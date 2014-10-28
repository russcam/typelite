using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using TypeLite.Extensions;
using TypeLite.ReadOnlyDictionary;
using TypeLite.TsModels;

namespace TypeLite {
    /// <summary>
    /// Generates TypeScript definitions form the code model.
    /// </summary>
    public class TsGenerator {
        private TsTypeFormatterCollection _formatter;
        private TypeConvertorCollection _convertor;
        private TsMemberIdentifierFormatter _memberFormatter;
        private TsMemberTypeFormatter _memberTypeFormatter;
        private TsTypeVisibilityFormatter _typeVisibilityFormatter;
        private TsModuleNameFormatter _moduleNameFormatter;
        private HashSet<TsClass> _generatedClasses;
        private HashSet<TsEnum> _generatedEnums;
        private List<string> _references;
        private Dictionary<string, string> _renamedModules;
        /// <summary>
        /// Gets collection of formatters for individual TsTypes
        /// </summary>
        public IReadOnlyDictionary<Type, TsTypeFormatter> Formaters {
            get {
                return new ReadOnlyDictionaryWrapper<Type, TsTypeFormatter>(_formatter._formatters);
            }
        }

        /// <summary>
        /// Initializes a new instance of the TsGenerator class with the default formatters.
        /// </summary>
        public TsGenerator() {
            _references = new List<string>();
            _generatedClasses = new HashSet<TsClass>();
            _generatedEnums = new HashSet<TsEnum>();

            _formatter = new TsTypeFormatterCollection();
            _formatter.RegisterTypeFormatter<TsClass>((type, formatter) => ((TsClass)type).Name);
            _formatter.RegisterTypeFormatter<TsSystemType>((type, formatter) => ((TsSystemType)type).Kind.ToTypeScriptString());
            _formatter.RegisterTypeFormatter<TsCollection>((type, formatter) =>
            {
                var itemType = ((TsCollection)type).ItemsType;
                var itemTypeAsClass = itemType as TsClass;
                if (itemTypeAsClass == null || !itemTypeAsClass.GenericArguments.Any()) return this.GetTypeName(itemType);
                return this.GetTypeName(itemType) + "<" + string.Join(",", itemTypeAsClass.GenericArguments.Select(this.GetTypeName)) + ">";
            });
            _formatter.RegisterTypeFormatter<TsEnum>((type, formatter) => ((TsEnum)type).Name);

            _convertor = new TypeConvertorCollection();

            _memberFormatter = (identifier) => identifier.Name;
            _memberTypeFormatter = (typeName, isTypeCollection, dimension) => typeName + (isTypeCollection ? string.Concat(Enumerable.Repeat("[]", dimension)) : "");
            _typeVisibilityFormatter = (typeName) => false;
            _moduleNameFormatter = (moduleName) => moduleName;
            _renamedModules = new Dictionary<string, string>();
        }

        /// <summary>
        /// Registers the formatter for the specific TsType
        /// </summary>
        /// <typeparam name="TFor">The type to register the formatter for. TFor is restricted to TsType and derived classes.</typeparam>
        /// <param name="formatter">The formatter to register</param>
        /// <remarks>
        /// If a formatter for the type is already registered, it is overwritten with the new value.
        /// </remarks>
        public void RegisterTypeFormatter<TFor>(TsTypeFormatter formatter) where TFor : TsType {
            _formatter.RegisterTypeFormatter<TFor>(formatter);
        }

        /// <summary>
        /// Registers the custom formatter for the TsClass type.
        /// </summary>
        /// <param name="formatter">The formatter to register.</param>
        public void RegisterTypeFormatter(TsTypeFormatter formatter) {
            _formatter.RegisterTypeFormatter<TsClass>(formatter);
        }

        /// <summary>
        /// Registers the converter for the specific Type
        /// </summary>
        /// <typeparam name="TFor">The type to register the converter for.</typeparam>
        /// <param name="convertor">The converter to register</param>
        /// <remarks>
        /// If a converter for the type is already registered, it is overwritten with the new value.
        /// </remarks>
        public void RegisterTypeConvertor<TFor>(TypeConvertor convertor) {
            _convertor.RegisterTypeConverter<TFor>(convertor);
        }

        /// <summary>
        /// Registers a formatter for class member identifiers.
        /// </summary>
        /// <param name="formatter">The formatter to register.</param>
        public void RegisterIdentifierFormatter(TsMemberIdentifierFormatter formatter) {
            _memberFormatter = formatter;
        }

        /// <summary>
        /// Registers a formatter for class member types.
        /// </summary>
        /// <param name="formatter">The formatter to register.</param>
        public void RegisterMemberTypeFormatter(TsMemberTypeFormatter formatter) {
            _memberTypeFormatter = formatter;
        }

        /// <summary>
        /// Registers a formatter for class member types.
        /// </summary>
        /// <param name="formatter">The formatter to register.</param>
        public void RegisterTypeVisibilityFormatter(TsTypeVisibilityFormatter formatter) {
            _typeVisibilityFormatter = formatter;
        }


        /// <summary>
        /// Registers a formatter for module names.
        /// </summary>
        /// <param name="formatter">The formatter to register.</param>
        public void RegisterModuleNameFormatter(TsModuleNameFormatter formatter) {
            _moduleNameFormatter = formatter;
        }

        /// <summary>
        /// Add a typescript reference
        /// </summary>
        /// <param name="reference">Name of d.ts file used as typescript reference</param>
        public void AddReference(string reference) {
            _references.Add(reference);
        }

        /// <summary>
        /// Generates TypeScript definitions for properties and enums in the model.
        /// </summary>
        /// <param name="model">The code model with classes to generate definitions for.</param>
        /// <returns>TypeScript definitions for classes in the model.</returns>
        public string Generate(TsModel model) {
            return this.Generate(model, TsGeneratorOutput.Properties | TsGeneratorOutput.Enums);
        }

        /// <summary>
        /// Generates TypeScript definitions for classes and/or enums in the model.
        /// </summary>
        /// <param name="model">The code model with classes to generate definitions for.</param>
        /// <param name="generatorOutput">The type of definitions to generate</param>
        /// <returns>TypeScript definitions for classes and/or enums in the model..</returns>
        public string Generate(TsModel model, TsGeneratorOutput generatorOutput) {
            var sb = new StringBuilder();

            if ((generatorOutput & TsGeneratorOutput.Properties) == TsGeneratorOutput.Properties
                || (generatorOutput & TsGeneratorOutput.Fields) == TsGeneratorOutput.Fields) {

                if ((generatorOutput & TsGeneratorOutput.Constants) == TsGeneratorOutput.Constants) {
                    // We can't generate constants together with properties or fields, because we can't set values in a .d.ts file.
                    throw new InvalidOperationException("Cannot generate constants together with properties or fields");
                }

                foreach (var reference in _references.Concat(model.References)) {
                    this.AppendReference(reference, sb);
                }
                sb.AppendLine();
            }

            foreach (var module in model.Modules) {
                this.AppendModule(module, sb, generatorOutput);
            }

            string result = sb.ToString();

            foreach (KeyValuePair<string, string> _renamedModule in _renamedModules) {
                result = result.Replace(_renamedModule.Key, _renamedModule.Value);
            }

            return result;
        }

        /// <summary>
        /// Generates reference to other d.ts file and appends it to the output.
        /// </summary>
        /// <param name="reference">The reference file to generate reference for.</param>
        /// <param name="sb">The output</param>
        private void AppendReference(string reference, StringBuilder sb) {
            sb.AppendFormat("/// <reference path=\"{0}\" />", reference);
            sb.AppendLine();
        }

        private void AppendModule(TsModule module, StringBuilder sb, TsGeneratorOutput generatorOutput) {
            var classes = module.Classes.Where(c => !_convertor.IsConvertorRegistered(c.ClrType) && !c.IsIgnored).ToList();
            var enums = module.Enums.Where(e => !_convertor.IsConvertorRegistered(e.ClrType) && !e.IsIgnored).ToList();
            if ((generatorOutput == TsGeneratorOutput.Enums && enums.Count == 0) ||
                (generatorOutput == TsGeneratorOutput.Properties && classes.Count == 0) ||
                (enums.Count == 0 && classes.Count == 0))
            {
                return;
            }

            string moduleName = GetModuleName(module.Name);
            if (moduleName != module.Name) {
                _renamedModules.Add(module.Name, moduleName);
            }

            if (generatorOutput != TsGeneratorOutput.Enums
                && (generatorOutput & TsGeneratorOutput.Constants) != TsGeneratorOutput.Constants) {
                sb.Append("declare ");
            }

            sb.AppendFormat("module {0} ", moduleName);
            sb.AppendLine("{");

            if ((generatorOutput & TsGeneratorOutput.Enums) == TsGeneratorOutput.Enums) {
                foreach (var enumModel in enums) {
                    this.AppendEnumDefinition(enumModel, sb, generatorOutput);
                }
            }

            if (((generatorOutput & TsGeneratorOutput.Properties) == TsGeneratorOutput.Properties)
                || (generatorOutput & TsGeneratorOutput.Fields) == TsGeneratorOutput.Fields) {
                foreach (var classModel in classes) {

                    this.AppendClassDefinition(classModel, sb, generatorOutput);
                }
            }

            if ((generatorOutput & TsGeneratorOutput.Constants) == TsGeneratorOutput.Constants) {
                foreach (var classModel in classes) {
                    if (classModel.IsIgnored) {
                        continue;
                    }

                    this.AppendConstantModule(classModel, sb);
                }
            }

            sb.AppendLine("}");
        }

        /// <summary>
        /// Generates class definition and appends it to the output.
        /// </summary>
        /// <param name="classModel">The class to generate definition for.</param>
        /// <param name="sb">The output.</param>
        /// <param name="generatorOutput"></param>
        private void AppendClassDefinition(TsClass classModel, StringBuilder sb, TsGeneratorOutput generatorOutput) {
            string typeName = this.GetTypeName(classModel);
            string visibility = this.GetTypeVisibility(typeName) ? "export " : "";
            sb.AppendFormat("{0}interface {1}", visibility, typeName);
            if (classModel.GenericArguments.Any()) {
                sb.AppendFormat("<{0}>", string.Join(",", classModel.GenericArguments.Select(arg => arg.ClrType.Name)));
            }
            if (classModel.BaseType != null) {
                sb.AppendFormat(" extends {0}", this.GetFullyQualifiedTypeName(classModel.BaseType, classModel.GenericArguments));
                var baseClassModel = classModel.BaseType as TsClass;
                if (baseClassModel != null && baseClassModel.GenericArguments != null && baseClassModel.GenericArguments.Any()) {
                    sb.AppendFormat("<{0}>", string.Join(",", baseClassModel.GenericArguments.Select(arg => this.GetFullyQualifiedTypeName(arg, null))));
                }
            }

            sb.AppendLine(" {");

            var members = new List<TsProperty>();
            if ((generatorOutput & TsGeneratorOutput.Properties) == TsGeneratorOutput.Properties) {
                members.AddRange(classModel.Properties);
            }
            if ((generatorOutput & TsGeneratorOutput.Fields) == TsGeneratorOutput.Fields) {
                members.AddRange(classModel.Fields);
            }

            foreach (var property in members) {
                if (property.IsIgnored) {
                    continue;
                }

                sb.AppendFormat("  {0}: {1}", this.GetPropertyName(property), this.GetPropertyType(property));
                if (!(property.PropertyType is TsCollection) && property.GenericArguments != null && property.GenericArguments.Any()) {
                    sb.AppendFormat("<{0}>", string.Join(", ", property.GenericArguments.Select(this.GetTypeName)));
                }
                sb.Append(";"); sb.AppendLine();
            }

            sb.AppendLine("}");

            _generatedClasses.Add(classModel);
        }

        private void AppendEnumDefinition(TsEnum enumModel, StringBuilder sb, TsGeneratorOutput output) {
            string typeName = this.GetTypeName(enumModel);
            string visibility = output == TsGeneratorOutput.Enums || (output & TsGeneratorOutput.Constants) == TsGeneratorOutput.Constants ? "export " : "";

            sb.AppendFormat("{0}enum {1} ", visibility, typeName);
            sb.AppendLine("{");

            int i = 1;
            foreach (var v in enumModel.Values) {
                sb.AppendFormat(i < enumModel.Values.Count ? "  {0} = {1}," : "  {0} = {1}", v.Name, v.Value);
                sb.AppendLine();
                i++;
            }

            sb.AppendLine("}");

            _generatedEnums.Add(enumModel);
        }

        /// <summary>
        /// Generates class definition and appends it to the output.
        /// </summary>
        /// <param name="classModel">The class to generate definition for.</param>
        /// <param name="sb">The output.</param>
        /// <param name="generatorOutput"></param>
        private void AppendConstantModule(TsClass classModel, StringBuilder sb)
        {
            if (!classModel.Constants.Any()) {
                return;
            }

            string typeName = this.GetTypeName(classModel);
            sb.AppendFormat("export module {0} ", typeName);

            sb.AppendLine("{");

            foreach (var property in classModel.Constants)
            {
                if (property.IsIgnored)
                {
                    continue;
                }

                sb.AppendFormat("  export var {0}: {1} = {2};", this.GetPropertyName(property), this.GetPropertyType(property), this.GetPropertyConstantValue(property));
                sb.AppendLine();
            }

            sb.AppendLine("}");

            _generatedClasses.Add(classModel);
        }

        /// <summary>
        /// Gets fully qualified name of the type
        /// </summary>
        /// <param name="type">The type to get name of</param>
        /// <param name="genericArguments"></param>
        /// <returns>Fully qualified name of the type</returns>
        private string GetFullyQualifiedTypeName(TsType type, IList<TsType> genericArguments) {
            var moduleName = string.Empty;

            if (type as TsModuleMember != null && !_convertor.IsConvertorRegistered(type.ClrType)) {
                var memberType = (TsModuleMember)type;
                moduleName = memberType.Module != null ? memberType.Module.Name : string.Empty;
            } else if (type as TsCollection != null) {
                var collectionType = (TsCollection)type;
                moduleName = GetCollectionModuleName(collectionType, moduleName);
            }

            if (type.ClrType.IsGenericParameter) {
                return this.GetTypeName(type);
            }
            if (!string.IsNullOrEmpty(moduleName)) {
                var name = moduleName + "." + this.GetTypeName(type);
                return name;
            }

            return this.GetTypeName(type);
        }

        /// <summary>
        /// Recursively finds the module name for the underlaying ItemsType of a TsCollection.
        /// </summary>
        /// <param name="collectionType">The TsCollection object.</param>
        /// <param name="moduleName">The module name.</param>
        /// <returns></returns>
        private string GetCollectionModuleName(TsCollection collectionType, string moduleName) {
            if (collectionType.ItemsType as TsModuleMember != null && !_convertor.IsConvertorRegistered(collectionType.ItemsType.ClrType)) {
                if (!collectionType.ItemsType.ClrType.IsGenericParameter)
                    moduleName = ((TsModuleMember) collectionType.ItemsType).Module != null ? ((TsModuleMember) collectionType.ItemsType).Module.Name : string.Empty;
            }
            if (collectionType.ItemsType as TsCollection != null) {
                moduleName = GetCollectionModuleName((TsCollection) collectionType.ItemsType, moduleName);
            }
            return moduleName;
        }

        /// <summary>
        /// Gets name of the type in the TypeScript
        /// </summary>
        /// <param name="type">The type to get name of</param>
        /// <returns>name of the type</returns>
        private string GetTypeName(TsType type) {
            if (_convertor.IsConvertorRegistered(type.ClrType)) {
                return _convertor.ConvertType(type.ClrType);
            }

            return _formatter.FormatType(type);
        }

        /// <summary>
        /// Gets property name in the TypeScript
        /// </summary>
        /// <param name="property">The property to get name of</param>
        /// <returns>name of the property</returns>
        private string GetPropertyName(TsProperty property) {
            var name = _memberFormatter(property);
            if (property.IsOptional) {
                name += "?";
            }

            return name;
        }

        /// <summary>
        /// Gets property type in the TypeScript
        /// </summary>
        /// <param name="property">The property to get type of</param>
        /// <returns>type of the property</returns>
        private string GetPropertyType(TsProperty property) {
            var asCollection = property.PropertyType as TsCollection;

            if (asCollection == null)
            {
                return _memberTypeFormatter(this.GetFullyQualifiedTypeName(property.PropertyType, property.GenericArguments), false);
            }
            else
            {
                return _memberTypeFormatter(this.GetFullyQualifiedTypeName(property.PropertyType, property.GenericArguments), true, asCollection.Dimension);
            }
        }

        /// <summary>
        /// Gets property constant value in TypeScript format
        /// </summary>
        /// <param name="property">The property to get constant value of</param>
        /// <returns>constant value of the property</returns>
        private string GetPropertyConstantValue(TsProperty property) {
            var quote = property.PropertyType.ClrType == typeof (string) ? "\"" : "";
            return quote + property.ConstantValue.ToString() + quote;
        }

        /// <summary>
        /// Gets whether a type should be marked with "Export" keyword in TypeScript
        /// </summary>
        /// <param name="typeName">The type to get the visibility of</param>
        /// <returns>bool indicating if type should be marked weith keyword "Export"</returns>
        private bool GetTypeVisibility(string typeName) {
            return _typeVisibilityFormatter(typeName);
        }

        /// <summary>
        /// Formats a module name
        /// </summary>
        /// <param name="moduleName">The module name to be formatted</param>
        /// <returns>The module name after formatting.</returns>
        private string GetModuleName(string moduleName) {
            return _moduleNameFormatter(moduleName);
        }

    }
}

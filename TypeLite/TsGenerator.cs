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
        protected TsTypeFormatterCollection _formatter;
        protected TypeConvertorCollection _convertor;
        protected TsMemberIdentifierFormatter _memberFormatter;
        protected TsMemberTypeFormatter _memberTypeFormatter;
        protected TsTypeVisibilityFormatter _typeVisibilityFormatter;
        protected TsModuleNameFormatter _moduleNameFormatter;
        protected HashSet<TsClass> _generatedClasses;
        protected HashSet<TsEnum> _generatedEnums;
        protected List<string> _references;
        protected Dictionary<string, string> _renamedModules;

        /// <summary>
        /// Gets collection of formatters for individual TsTypes
        /// </summary>
        public IReadOnlyDictionary<Type, TsTypeFormatter> Formaters {
            get {
                return new ReadOnlyDictionaryWrapper<Type, TsTypeFormatter>(_formatter._formatters);
            }
        }

        /// <summary>
        /// Gets or sets string for the single indentation level.
        /// </summary>
        public string IndentationString { get; set; }

        /// <summary>
        /// Initializes a new instance of the TsGenerator class with the default formatters.
        /// </summary>
        public TsGenerator() {
            _references = new List<string>();
            _generatedClasses = new HashSet<TsClass>();
            _generatedEnums = new HashSet<TsEnum>();

            _formatter = new TsTypeFormatterCollection();
            _formatter.RegisterTypeFormatter<TsClass>((type, formatter) => {
                var tsClass = ((TsClass)type);
                if (!tsClass.GenericArguments.Any()) return tsClass.Name;
                return tsClass.Name + "<" + string.Join(", ", tsClass.GenericArguments.Select(a => a as TsCollection != null ? this.GetFullyQualifiedTypeName(a) + "[]" : this.GetFullyQualifiedTypeName(a))) + ">";
            });
            _formatter.RegisterTypeFormatter<TsSystemType>((type, formatter) => ((TsSystemType)type).Kind.ToTypeScriptString());
            _formatter.RegisterTypeFormatter<TsCollection>((type, formatter) => {
                var itemType = ((TsCollection)type).ItemsType;
                var itemTypeAsClass = itemType as TsClass;
                if (itemTypeAsClass == null || !itemTypeAsClass.GenericArguments.Any()) return this.GetTypeName(itemType);
                return this.GetTypeName(itemType);
            });
            _formatter.RegisterTypeFormatter<TsEnum>((type, formatter) => ((TsEnum)type).Name);

            _convertor = new TypeConvertorCollection();

            _memberFormatter = (identifier) => identifier.Name;
            _memberTypeFormatter = (typeName, isTypeCollection, dimension) => typeName + (isTypeCollection ? string.Concat(Enumerable.Repeat("[]", dimension)) : "");
            _typeVisibilityFormatter = (typeName) => false;
            _moduleNameFormatter = (moduleName) => moduleName;
            _renamedModules = new Dictionary<string, string>();

            this.IndentationString = "\t";
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
            var sb = new ScriptBuilder(this.IndentationString);

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
        private void AppendReference(string reference, ScriptBuilder sb) {
            sb.AppendFormat("/// <reference path=\"{0}\" />", reference);
            sb.AppendLine();
        }

        private void AppendModule(TsModule module, ScriptBuilder sb, TsGeneratorOutput generatorOutput) {
            var classes = module.Classes.Where(c => !_convertor.IsConvertorRegistered(c.Type) && !c.IsIgnored).ToList();
            var enums = module.Enums.Where(e => !_convertor.IsConvertorRegistered(e.Type) && !e.IsIgnored).ToList();
            if ((generatorOutput == TsGeneratorOutput.Enums && enums.Count == 0) ||
                (generatorOutput == TsGeneratorOutput.Properties && classes.Count == 0) ||
                (enums.Count == 0 && classes.Count == 0)) {
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

            sb.AppendLine(string.Format("module {0} {{", moduleName));
            using (sb.IncreaseIndentation()) {
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
            }

            sb.AppendLine("}");
        }

        /// <summary>
        /// Generates class definition and appends it to the output.
        /// </summary>
        /// <param name="classModel">The class to generate definition for.</param>
        /// <param name="sb">The output.</param>
        /// <param name="generatorOutput"></param>
        private void AppendClassDefinition(TsClass classModel, ScriptBuilder sb, TsGeneratorOutput generatorOutput) {
            string typeName = this.GetTypeName(classModel);
            string visibility = this.GetTypeVisibility(typeName) ? "export " : "";
            sb.AppendFormatIndented("{0}interface {1}", visibility, typeName);
            if (classModel.BaseType != null) {
                sb.AppendFormat(" extends {0}", this.GetFullyQualifiedTypeName(classModel.BaseType));
            }

            sb.AppendLine(" {");

            var members = new List<TsProperty>();
            if ((generatorOutput & TsGeneratorOutput.Properties) == TsGeneratorOutput.Properties) {
                members.AddRange(classModel.Properties);
            }
            if ((generatorOutput & TsGeneratorOutput.Fields) == TsGeneratorOutput.Fields) {
                members.AddRange(classModel.Fields);
            }
            using (sb.IncreaseIndentation()) {
                foreach (var property in members) {
                    if (property.IsIgnored) {
                        continue;
                    }

                    sb.AppendLineIndented(string.Format("{0}: {1};", this.GetPropertyName(property), this.GetPropertyType(property)));
                }
            }

            sb.AppendLineIndented("}");

            _generatedClasses.Add(classModel);
        }

        protected void AppendEnumDefinition(TsEnum enumModel, ScriptBuilder sb, TsGeneratorOutput output) {
            string typeName = this.GetTypeName(enumModel);
            string visibility = output == TsGeneratorOutput.Enums || (output & TsGeneratorOutput.Constants) == TsGeneratorOutput.Constants ? "export " : "";

            sb.AppendLineIndented(string.Format("{0}enum {1} {{", visibility, typeName));

            using (sb.IncreaseIndentation()) {
                int i = 1;
                foreach (var v in enumModel.Values) {
                    sb.AppendLineIndented(string.Format(i < enumModel.Values.Count ? "{0} = {1}," : "{0} = {1}", v.Name, v.Value));
                    i++;
                }
            }

            sb.AppendLineIndented("}");

            _generatedEnums.Add(enumModel);
        }

        /// <summary>
        /// Generates class definition and appends it to the output.
        /// </summary>
        /// <param name="classModel">The class to generate definition for.</param>
        /// <param name="sb">The output.</param>
        /// <param name="generatorOutput"></param>
        private void AppendConstantModule(TsClass classModel, ScriptBuilder sb) {
            if (!classModel.Constants.Any()) {
                return;
            }

            string typeName = this.GetTypeName(classModel);
            sb.AppendLineIndented(string.Format("export module {0} {{", typeName));

            using (sb.IncreaseIndentation()) {
                foreach (var property in classModel.Constants) {
                    if (property.IsIgnored) {
                        continue;
                    }

                    sb.AppendFormatIndented("export var {0}: {1} = {2};", this.GetPropertyName(property), this.GetPropertyType(property), this.GetPropertyConstantValue(property));
                    sb.AppendLine();
                }

            }
            sb.AppendLineIndented("}");

            _generatedClasses.Add(classModel);
        }

        /// <summary>
        /// Gets fully qualified name of the type
        /// </summary>
        /// <param name="type">The type to get name of</param>
        /// <returns>Fully qualified name of the type</returns>
        private string GetFullyQualifiedTypeName(TsType type) {
            var moduleName = string.Empty;

            if (type as TsModuleMember != null && !_convertor.IsConvertorRegistered(type.Type)) {
                var memberType = (TsModuleMember)type;
                moduleName = memberType.Module != null ? memberType.Module.Name : string.Empty;
            } else if (type as TsCollection != null) {
                var collectionType = (TsCollection)type;
                moduleName = GetCollectionModuleName(collectionType, moduleName);
            }

            if (type.Type.IsGenericParameter) {
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
            if (collectionType.ItemsType as TsModuleMember != null && !_convertor.IsConvertorRegistered(collectionType.ItemsType.Type)) {
                if (!collectionType.ItemsType.Type.IsGenericParameter)
                    moduleName = ((TsModuleMember)collectionType.ItemsType).Module != null ? ((TsModuleMember)collectionType.ItemsType).Module.Name : string.Empty;
            }
            if (collectionType.ItemsType as TsCollection != null) {
                moduleName = GetCollectionModuleName((TsCollection)collectionType.ItemsType, moduleName);
            }
            return moduleName;
        }

        /// <summary>
        /// Gets name of the type in the TypeScript
        /// </summary>
        /// <param name="type">The type to get name of</param>
        /// <returns>name of the type</returns>
        protected string GetTypeName(TsType type) {
            if (_convertor.IsConvertorRegistered(type.Type)) {
                return _convertor.ConvertType(type.Type);
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

            if (asCollection == null) {
                return _memberTypeFormatter(this.GetFullyQualifiedTypeName(property.PropertyType), false);
            } else {
                return _memberTypeFormatter(this.GetFullyQualifiedTypeName(property.PropertyType), true, asCollection.Dimension);
            }
        }

        /// <summary>
        /// Gets property constant value in TypeScript format
        /// </summary>
        /// <param name="property">The property to get constant value of</param>
        /// <returns>constant value of the property</returns>
        private string GetPropertyConstantValue(TsProperty property) {
            var quote = property.PropertyType.Type == typeof(string) ? "\"" : "";
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
        protected string GetModuleName(string moduleName) {
            return _moduleNameFormatter(moduleName);
        }

    }
}

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
			_formatter.RegisterTypeFormatter<TsCollection>((type, formatter) => this.GetTypeName(((TsCollection)type).ItemsType));
			_formatter.RegisterTypeFormatter<TsEnum>((type, formatter) => ((TsEnum)type).Name);

			_convertor = new TypeConvertorCollection();

			_memberFormatter = (identifier) => identifier.Name;
            _memberTypeFormatter = (typeName, isTypeCollection) => typeName + (isTypeCollection ? "[]" : "");
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
		/// Generates TypeScript definitions for classes in the model.
		/// </summary>
		/// <param name="model">The code model with classes to generate definitions for.</param>
		/// <returns>TypeScript definitions for classes in the model.</returns>
		public string Generate(TsModel model) {
			var sb = new StringBuilder();

			foreach (var reference in _references.Concat(model.References)) {
				this.AppendReference(reference, sb);
			}
			sb.AppendLine();

			foreach (var module in model.Modules) {
				this.AppendModule(module, sb);
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

		private void AppendModule(TsModule module, StringBuilder sb) {
            var classes = module.Classes.Where(c => !_convertor.IsConvertorRegistered(c.ClrType)).ToList();
            var enums = module.Enums.Where(e => !_convertor.IsConvertorRegistered(e.ClrType)).ToList();
            if (enums.Count == 0 && classes.Count == 0)
                return;

            string moduleName = GetModuleName(module.Name);
            if (moduleName != module.Name) {
                _renamedModules.Add(module.Name, moduleName);
            }

            sb.AppendFormat("declare module {0} ", moduleName);
			sb.AppendLine("{");

			foreach (var enumModel in enums) {
				if (enumModel.IsIgnored) {
					continue;
				}
				this.AppendEnumDefinition(enumModel, sb);
			}

			foreach (var classModel in classes) {
				if (classModel.IsIgnored) {
					continue;
				}
                
				this.AppendClassDefinition(classModel, sb);
			}

			sb.AppendLine("}");
		}

		/// <summary>
		/// Generates class definition and appends it to the output.
		/// </summary>
		/// <param name="classModel">The class to generate definition for.</param>
		/// <param name="sb">The output.</param>
		private void AppendClassDefinition(TsClass classModel, StringBuilder sb) {
            string typeName = this.GetTypeName(classModel);
            string visibility = GetTypeVisibility(typeName) ? "export " : "";
            sb.AppendFormat("{0}interface {1} ", visibility, typeName);
			if (classModel.BaseType != null) {
				sb.AppendFormat("extends {0} ", this.GetFullyQualifiedTypeName(classModel.BaseType));
			}

			sb.AppendLine("{");

			foreach (var property in classModel.Properties) {
				if (property.IsIgnored) {
					continue;
				}

				sb.AppendFormat("  {0}: {1};", this.GetPropertyName(property), this.GetPropertyType(property));
				sb.AppendLine();
			}

			sb.AppendLine("}");

			_generatedClasses.Add(classModel);
		}

		private void AppendEnumDefinition(TsEnum enumModel, StringBuilder sb) {
            string typeName = this.GetTypeName(enumModel);
            string visibility = GetTypeVisibility(typeName) ? "export " : "";
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
		/// Gets fully qualified name of the type
		/// </summary>
		/// <param name="type">The type to get name of</param>
		/// <returns>Fully qualified name of the type</returns>
		private string GetFullyQualifiedTypeName(TsType type) {
			var moduleName = string.Empty;

			if (type as TsModuleMember != null && !_convertor.IsConvertorRegistered(type.ClrType)) {
				var memberType = (TsModuleMember)type;
				moduleName = memberType.Module != null ? memberType.Module.Name : string.Empty;
			} else if (type as TsCollection != null) {
				var collectionType = (TsCollection)type;
				if (collectionType.ItemsType as TsModuleMember != null && !_convertor.IsConvertorRegistered(collectionType.ItemsType.ClrType)) {
					moduleName = ((TsModuleMember)collectionType.ItemsType).Module != null ? ((TsModuleMember)collectionType.ItemsType).Module.Name : string.Empty;
				}
			}

			if (!string.IsNullOrEmpty(moduleName)) {
				return moduleName + "." + this.GetTypeName(type);
			}

			return this.GetTypeName(type);
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
            return _memberTypeFormatter(this.GetFullyQualifiedTypeName(property.PropertyType), property.PropertyType is TsCollection);
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

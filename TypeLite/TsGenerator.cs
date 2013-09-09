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
		private TsMemberIdentifierFormatter _memberFormatter;
		private HashSet<TsClass> _generatedClasses;
	    private HashSet<TsEnum> _generatedEnums;

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
			_generatedClasses = new HashSet<TsClass>();
            _generatedEnums = new HashSet<TsEnum>();

			_formatter = new TsTypeFormatterCollection();
			_formatter.RegisterTypeFormatter<TsClass>((type, formatter) => ((TsClass)type).Name);
			_formatter.RegisterTypeFormatter<TsSystemType>((type, formatter) => ((TsSystemType)type).Kind.ToTypeScriptString());
			_formatter.RegisterTypeFormatter<TsCollection>((type, formatter) => formatter.FormatType(((TsCollection)type).ItemsType) + "[]");
            _formatter.RegisterTypeFormatter<TsEnum>((type,formatter)=>((TsEnum)type).Name);
			_memberFormatter = (identifier) => identifier.Name;
		}

		/// <summary>
		/// Registers the formatter for the specific TsType
		/// </summary>
		/// <typeparam name="TFor">The type to register the formatter for. TFor is restricted to TsType and derived classes.</typeparam>
		/// <param name="formatter">The formatter to register</param>
		/// <remarks>
		/// If a formatter for the type is already registered, it is overwriten to the new value.
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
		/// Registers a formatter for class member identifiers.
		/// </summary>
		/// <param name="formatter">The formater to register.</param>
		public void RegisterIdentifierFormatter(TsMemberIdentifierFormatter formatter) {
			_memberFormatter = formatter;
		}

		/// <summary>
		/// Generates TypeScript definitions for classes in the model.
		/// </summary>
		/// <param name="model">The code model with classes to generate definitions for.</param>
		/// <returns>TypeScript definitions for classes in the model.</returns>
		public string Generate(TsModel model) {
			var sb = new StringBuilder();

			foreach (var reference in model.References) {
				this.AppendReference(reference, sb);
			}

			sb.AppendLine();

          


            foreach (var enumModel in model.Enums)
            {
                if (!enumModel.IsIgnored)
                {
                    var module = model.Modules.SingleOrDefault(m => m.Name == enumModel.Module.Name);
                    if (module == null)
                        model.Modules.Add(enumModel.Module);
                    else
                        module.AddEnum(enumModel);
                    
                }
            }

            foreach (var classModel in model.Classes)
            {
                if (!classModel.IsIgnored)
                {
                    var module = model.Modules.SingleOrDefault(m => m.Name == classModel.Module.Name);
                     if (module == null)
                        model.Modules.Add(classModel.Module);
                     else
                        module.AddClass(classModel);
                }

            }

            foreach (var module in model.Modules)
            {
                this.AppendModule(module, sb);
            }
          

           
			return sb.ToString();

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
			sb.AppendFormat("declare module {0} ", module.Name);
			sb.AppendLine("{");

            foreach (var enumModel in module.Enums)
            {
                if (enumModel.IsIgnored)
                {
                    continue;
                }

                this.AppendEnumDefinition(enumModel,sb);
                    
            }

			foreach (var classModel in module.Classes) {
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
			sb.AppendFormat("interface {0} ", _formatter.FormatType(classModel));
			if (classModel.BaseType != null) {
				sb.AppendFormat("extends {0} ", this.GetFullyQualifiedTypeName(classModel.BaseType));
			}

			sb.AppendLine("{");

			foreach (var property in classModel.Properties) {
				if (property.IsIgnored) {
					continue;
				}

				sb.AppendFormat("  {0}: {1};", _memberFormatter(property), this.GetFullyQualifiedTypeName(property.PropertyType));
				sb.AppendLine();
			}

			sb.AppendLine("}");

			_generatedClasses.Add(classModel);
		}

        private void AppendEnumDefinition(TsEnum enumModel, StringBuilder sb)
        {
            sb.AppendFormat("enum {0} ", _formatter.FormatType(enumModel));
           

            sb.AppendLine("{");

            int i = 1;
            foreach (var v in enumModel.Values)
            {
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

			if (type as TsClass != null) {
				var classType = (TsClass)type;
				moduleName = classType.Module != null ? classType.Module.Name : string.Empty;
			} else if (type as TsCollection != null) {
				var collectionType = (TsCollection)type;
				if (collectionType.ItemsType as TsClass != null) {
					moduleName = ((TsClass)collectionType.ItemsType).Module != null ? ((TsClass)collectionType.ItemsType).Module.Name : string.Empty;
				}
			}

            if (type as TsEnum!=null)
            {
                var enumType = (TsEnum) type;
                moduleName = enumType.Module != null ? enumType.Module.Name : string.Empty;
            }

			if (!string.IsNullOrEmpty(moduleName)) {
				return moduleName + "." + _formatter.FormatType(type);
			}

			return _formatter.FormatType(type);
		}
	}
}

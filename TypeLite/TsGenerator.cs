using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

			_formatter = new TsTypeFormatterCollection();
			_formatter.RegisterTypeFormatter<TsClass>((type, formatter) => ((TsClass)type).Name);
			_formatter.RegisterTypeFormatter<TsSystemType>((type, formatter) => ((TsSystemType)type).Kind.ToString().ToLower());
			_formatter.RegisterTypeFormatter<TsCollection>((type, formatter) => formatter.FormatType(((TsCollection)type).ItemsType) + "[]");

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

			foreach (var module in model.Modules) {
				this.AppendModule(module, sb);
			}

			foreach (var classModel in model.Classes) {
				if (classModel.IsIgnored || _generatedClasses.Contains(classModel)) {
					continue;
				}

				this.AppendClassDefinition(classModel, sb);
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
			sb.AppendFormat("module {0} ", module.Name);
			sb.AppendLine("{");

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
				sb.AppendFormat("extends {0} ", _formatter.FormatType(classModel.BaseType));
			}

			sb.AppendLine("{");

			foreach (var property in classModel.Properties) {
				if (property.IsIgnored) {
					continue;
				}

				sb.AppendFormat("  {0}: {1};", _memberFormatter(property), _formatter.FormatType(property.PropertyType));
				sb.AppendLine();
			}

			sb.AppendLine("}");

			_generatedClasses.Add(classModel);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TypeLite.Extensions;

namespace TypeLite.TsModels {
	/// <summary>
	/// Represents an enum in the code model.
	/// </summary>
	public class TsEnum : TsType {
		private TsModule _module;

		/// <summary>
		/// Gets or sets the name of the enum.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets bool value indicating whether this enum will be ignored by TsGenerator.
		/// </summary>
		public bool IsIgnored { get; set; }

		/// <summary>
		/// Gets collection of properties of the class.
		/// </summary>
		public ICollection<TsEnumValue> Values { get; private set; }

		/// <summary>
		/// Gets or sets module, that contains this class.
		/// </summary>
		public TsModule Module {
			get {
				return _module;
			}
			set {
				if (_module != null) {
					_module.AddEnum(this);
				}
				_module = value;
				if (_module != null) {
					_module.AddEnum(this);
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the TsEnum class with the specific CLR enum.
		/// </summary>
		/// <param name="clrType">The CLR enum represented by this instance of the TsEnum.</param>
		public TsEnum(Type clrType)
			: base(clrType) {
			if (!clrType.IsEnum) {
				throw new ArgumentException("ClrType isn't enum.");
			}
			this.Name = clrType.Name;
			this.Values = new List<TsEnumValue>(this.GetEnumValues(clrType));
			this.Module = new TsModule(clrType.Namespace);

			var attribute = this.ClrType.GetCustomAttribute<TsEnumAttribute>(false);
			if (attribute != null) {
				if (!string.IsNullOrEmpty(attribute.Name)) {
					this.Name = attribute.Name;
				}

				if (!string.IsNullOrEmpty(attribute.Module)) {
					this.Module.Name = attribute.Module;
				}
			}
		}

		/// <summary>
		/// Retrieves a collection of possible value of the enum.
		/// </summary>
		/// <param name="clrType">The type of the enum.</param>
		/// <returns>collection of all enum values.</returns>
		protected IEnumerable<TsEnumValue> GetEnumValues(Type clrType) {
			return clrType.GetFields()
				.Where(field => field.IsLiteral && !string.IsNullOrEmpty(field.Name))
				.Select(field => new TsEnumValue(field.Name, (int)field.GetValue(null)));
		}
	}
}
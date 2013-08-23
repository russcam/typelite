using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeLite.TsModels {
	/// <summary>
	/// Represents an enum in the code model.
	/// </summary>
	public class TsEnum : TsType {
		/// <summary>
		/// Gets or sets the name of the enum.
		/// </summary>
		public string Name { get; set; }

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

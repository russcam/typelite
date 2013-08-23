using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeLite.TsModels {
	/// <summary>
	/// Represents a value of the enum
	/// </summary>
	public class TsEnumValue {
		/// <summary>
		/// Gets or sets name of the enum value
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets value of the enum
		/// </summary>
		public int Value { get; set; }

		/// <summary>
		/// Initializes a new instance of the TsEnumValue class.
		/// </summary>
		public TsEnumValue() {
		}

		/// <summary>
		/// Initializes a enw instance of the TsEnumValue class with the specific name and value.
		/// </summary>
		/// <param name="name">The name of the enum value.</param>
		/// <param name="value">The value of the enum value.</param>
		public TsEnumValue(string name, int value) {
			this.Name = name;
			this.Value = value;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeLite {
	/// <summary>
	/// Configures properties of the property in the script model.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class TsPropertyAttribute : Attribute {
		/// <summary>
		/// Gets or sets name of the property in script model.
		/// </summary>
		public string Name { get; set; }
	}
}

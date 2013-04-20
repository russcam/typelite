using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeLite.Extensions;

namespace TypeLite.TsModels {
	/// <summary>
	/// Represents a property of the class in the code model.
	/// </summary>
	public class TsProperty : IMemberIdentifier {
		/// <summary>
		/// Gets or sets name of the property.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets type of the property.
		/// </summary>
		public TsType PropertyType { get; set; }

		/// <summary>
		/// Gets the CLR property represented by this TsProperty.
		/// </summary>
		public PropertyInfo ClrProperty { get; private set; }

		/// <summary>
		/// Gets or sets bool value indicating whether this property will be ignored by TsGenerator.
		/// </summary>
		public bool IsIgnored { get; set; }

		/// <summary>
		/// Initializes a new instance of the TsProperty class with the specific CLR property.
		/// </summary>
		/// <param name="clrProperty">The CLR preperty represented by this instance of the TsProperty.</param>
		public TsProperty(PropertyInfo clrProperty) {
			this.ClrProperty = clrProperty;
			this.Name = clrProperty.Name;

			if (clrProperty.ReflectedType.IsGenericType) {
				var definitionType = clrProperty.ReflectedType.GetGenericTypeDefinition();
				var definitionTypeProperty = definitionType.GetProperty(clrProperty.Name);
				if (definitionTypeProperty.PropertyType.IsGenericParameter) {
					this.PropertyType = TsType.Any;
				} else {
					this.PropertyType = new TsType(clrProperty.PropertyType);
				}
			} else {
				this.PropertyType = new TsType(clrProperty.PropertyType);
			}

			var attribute = clrProperty.GetCustomAttribute<TsPropertyAttribute>(false);
			if (attribute != null) {
				if (!string.IsNullOrEmpty(attribute.Name)) {
					this.Name = attribute.Name;
				}
			}

			this.IsIgnored = (clrProperty.GetCustomAttribute<TsIgnoreAttribute>(false) != null);
		}
	}
}

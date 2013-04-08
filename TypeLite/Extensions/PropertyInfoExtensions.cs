using System;
using System.Linq;
using System.Reflection;

namespace TypeLite.Extensions {
	/// <summary>
	/// Contains extensions for PropertyInfo class
	/// </summary>
	public static class PropertyInfoExtensions {
		/// <summary>
		/// Retrieves a custom attribute of a specified type that is applied to a specified property.
		/// </summary>
		/// <typeparam name="TType">The type of attribute to search for.</typeparam>
		/// <param name="propertyInfo">The property to inspect.</param>
		/// <param name="inherit">true to search this member's inheritance chain to find the attributes; otherwise, false. This parameter is ignored for properties and events; see Remarks.</param>
		/// <returns>A custom attribute that matches T, or null if no such attribute is found.</returns>
		public static TType GetCustomAttribute<TType>(this PropertyInfo propertyInfo, bool inherit) where TType : Attribute {
			return propertyInfo.GetCustomAttributes(typeof(TType), inherit).FirstOrDefault() as TType;
		}
	}
}

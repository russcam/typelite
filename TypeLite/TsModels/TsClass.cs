using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using TypeLite.Extensions;

namespace TypeLite.TsModels {
	/// <summary>
	/// Represents a class in the code model.
	/// </summary>
	public class TsClass : TsModuleMember {
		/// <summary>
		/// Gets collection of properties of the class.
		/// </summary>
		public ICollection<TsProperty> Properties { get; private set; }

		/// <summary>
		/// Gets base type of the class
		/// </summary>
		/// <remarks>
		/// If the class derives from the object, the BaseType property is null.
		/// </remarks>
		public TsType BaseType { get; internal set; }

		/// <summary>
		/// Gets or sets bool value indicating whether this class will be ignored by TsGenerator.
		/// </summary>
		public bool IsIgnored { get; set; }

		/// <summary>
		/// Initializes a new instance of the TsClass class with the specific CLR type.
		/// </summary>
		/// <param name="clrType">The CLR type represented by this instance of the TsClass</param>
		public TsClass(Type clrType)
			: base(clrType) {

			this.Properties = this.ClrType
				.GetProperties()
				.Where(pi => pi.DeclaringType == this.ClrType)
				.Select(pi => new TsProperty(pi))
				.ToList();

			if (clrType.IsGenericType) {
				this.Name = clrType.Name.Remove(clrType.Name.IndexOf('`'));
			} else {
				this.Name = clrType.Name;
			}

			if (this.ClrType.BaseType != null && this.ClrType.BaseType != typeof(object) && this.ClrType.BaseType != typeof(ValueType)) {
				this.BaseType = new TsType(this.ClrType.BaseType);
			}

			var attribute = this.ClrType.GetCustomAttribute<TsClassAttribute>(false);
			if (attribute != null) {
				if (!string.IsNullOrEmpty(attribute.Name)) {
					this.Name = attribute.Name;
				}

				if (!string.IsNullOrEmpty(attribute.Module)) {
					this.Module.Name = attribute.Module;
				}
			}
		}
	}
}

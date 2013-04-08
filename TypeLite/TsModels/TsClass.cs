using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using TypeLite.Extensions;

namespace TypeLite.TsModels {
	/// <summary>
	/// Represents a class in the code model.
	/// </summary>
	public class TsClass : TsType {
        private TsModule _module;

		/// <summary>
		/// Gets collection of properties of the class.
		/// </summary>
		public ICollection<TsProperty> Properties { get; private set; }

        /// <summary>
        /// Gets or sets module, that contains this class.
        /// </summary>
        public TsModule Module {
            get {
                return _module;
            }
            set {
                if (_module != null) {
                    _module.RemoveClass(this);
                }
                _module = value;
                if (_module != null) {
                    _module.AddClass(this);
                }
            }
        }

		/// <summary>
		/// Gets base type of the class
		/// </summary>
		/// <remarks>
		/// If the class derives from the object, the BaseType property is null.
		/// </remarks>
		public TsType BaseType { get; internal set; }

		/// <summary>
		/// Gets or sets the name of the class.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets bool value indicating whether this class will be ignored by TsGenerator.
		/// </summary>
		public bool IsIgnored { get; set; }

		/// <summary>
		/// Initializes a new instance of the TsClass class with the specific CLR type.
		/// </summary>
		/// <param name="clrType">The CLR type represented by this instance of the TsClass</param>
		public TsClass(Type clrType) : base(clrType) {
			this.Properties = clrType
				.GetProperties()
				.Where(pi => pi.DeclaringType == clrType)
				.Select(pi => new TsProperty(pi))
				.ToList();
			this.Name = clrType.Name;
			this.Module = new TsModule(clrType.Namespace);

			if (clrType.BaseType != null && clrType.BaseType != typeof(object)) {
				this.BaseType = new TsType(clrType.BaseType);
			}

			var attribute = clrType.GetCustomAttribute<TsClassAttribute>(false);
			if (attribute != null) {
				if (!string.IsNullOrEmpty(attribute.Name)){ 
					this.Name = attribute.Name;
				}

				if (!string.IsNullOrEmpty(attribute.Module)) {
					this.Module.Name = attribute.Module;
				}
			}
		}
	}
}

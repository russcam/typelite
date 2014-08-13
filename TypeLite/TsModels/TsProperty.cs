using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
        public MemberInfo ClrProperty { get; private set; }

        /// <summary>
        /// Gets or sets bool value indicating whether this property will be ignored by TsGenerator.
        /// </summary>
        public bool IsIgnored { get; set; }

        /// <summary>
        /// Gets or sets bool value indicating whether this property is optional in TypeScript interface.
        /// </summary>
        public bool IsOptional { get; set;}

        /// <summary>
        /// Gets or sets the constant value of this property.
        /// </summary>
        public object ConstantValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the TsProperty class with the specific CLR property.
        /// </summary>
        /// <param name="clrProperty">The CLR property represented by this instance of the TsProperty.</param>
        public TsProperty(PropertyInfo clrProperty) {
            this.ClrProperty = clrProperty;
            this.Name = clrProperty.Name;

            if (clrProperty.ReflectedType.IsGenericType) {
                var definitionType = clrProperty.ReflectedType.GetGenericTypeDefinition();
                var definitionTypeProperty = definitionType.GetProperty(clrProperty.Name);
                if (definitionTypeProperty.PropertyType.IsGenericParameter) {
                    this.PropertyType = TsType.Any;
                } else {
                    this.PropertyType = clrProperty.PropertyType.IsEnum ? new TsEnum(clrProperty.PropertyType) : new TsType(clrProperty.PropertyType);
                }
            } else {
                var propertyType = clrProperty.PropertyType;
                if (propertyType.IsNullable()) {
                    propertyType = propertyType.GetNullableValueType();
                }

                this.PropertyType = propertyType.IsEnum ? new TsEnum(propertyType) : new TsType(propertyType);
            }

            var attribute = clrProperty.GetCustomAttribute<TsPropertyAttribute>(false);
            if (attribute != null) {
                if (!string.IsNullOrEmpty(attribute.Name)) {
                    this.Name = attribute.Name;
                }

                this.IsOptional = attribute.IsOptional;
            }

            this.IsIgnored = (clrProperty.GetCustomAttribute<TsIgnoreAttribute>(false) != null);

            // Only fields can be constants.
            this.ConstantValue = null;
        }

        /// <summary>
        /// Initializes a new instance of the TsProperty class with the specific CLR field.
        /// </summary>
        /// <param name="clrProperty">The CLR field represented by this instance of the TsProperty.</param>
        public TsProperty(FieldInfo clrProperty) {
            this.ClrProperty = clrProperty;
            this.Name = clrProperty.Name;

            if (clrProperty.ReflectedType.IsGenericType) {
                var definitionType = clrProperty.ReflectedType.GetGenericTypeDefinition();
                var definitionTypeProperty = definitionType.GetProperty(clrProperty.Name);
                if (definitionTypeProperty.PropertyType.IsGenericParameter) {
                    this.PropertyType = TsType.Any;
                } else {
                    this.PropertyType = clrProperty.FieldType.IsEnum ? new TsEnum(clrProperty.FieldType) : new TsType(clrProperty.FieldType);
                }
            } else {
                var propertyType = clrProperty.FieldType;
                if (propertyType.IsNullable()) {
                    propertyType = propertyType.GetNullableValueType();
                }

                this.PropertyType = propertyType.IsEnum ? new TsEnum(propertyType) : new TsType(propertyType);
			}

			var attribute = clrProperty.GetCustomAttribute<TsPropertyAttribute>(false);
			if (attribute != null) {
				if (!string.IsNullOrEmpty(attribute.Name)) {
					this.Name = attribute.Name;
				}

				this.IsOptional = attribute.IsOptional;
			}

			this.IsIgnored = (clrProperty.GetCustomAttribute<TsIgnoreAttribute>(false) != null);

            if (clrProperty.IsLiteral && !clrProperty.IsInitOnly) {
                // it's a constant
                this.ConstantValue = clrProperty.GetValue(null);
            } else {
                // not a constant
                this.ConstantValue = null;
            }
        }
	}
}

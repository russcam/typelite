using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeLite.Extensions;

namespace TypeLite.TsModels {
	/// <summary>
	/// Represents a type in the code model.
	/// </summary>
	public class TsType {
		/// <summary>
		/// Gets the CLR type represented by this instance of the TsType.
		/// </summary>
		public Type ClrType { get; private set; }

		/// <summary>
		/// Initializes a new instance of the TsType class with the specific CLR type.
		/// </summary>
		/// <param name="clrType">The CLR type represented by this instance of the TsType.</param>
		public TsType(Type clrType) {
			if (clrType.IsNullable()) {
				clrType = clrType.GetNullableValueType();
			}

			this.ClrType = clrType;
		}

		/// <summary>
		/// Represents the TsType for the object CLR type.
		/// </summary>
		public static readonly TsType Any = new TsType(typeof(object));

		/// <summary>
		/// Gets TsTypeFamily of the CLR type.
		/// </summary>
		/// <param name="type">The CLR type to get TsTypeFamily of</param>
		/// <returns>TsTypeFamily of the CLR type</returns>
		internal static TsTypeFamily GetTypeFamily(System.Type type) {
			if (type.IsNullable()) {
				return TsType.GetTypeFamily(type.GetNullableValueType());
			}

			var isString = (type == typeof(string));
			var isEnumerable = typeof(IEnumerable).IsAssignableFrom(type);

			// surprisingly  Decimal isn't a primitive type
            if (isString || type.IsPrimitive || type.FullName == "System.Decimal" || type.FullName == "System.DateTime" || type.FullName == "System.DateTimeOffset") {
				return TsTypeFamily.System;
			} else if (isEnumerable) {
				return TsTypeFamily.Collection;
			}

			if (type.IsEnum) {
				return TsTypeFamily.Enum;
			}

			if ((type.IsClass && type.FullName != "System.Object") || type.IsValueType /* structures */ || type.IsInterface) {
				return TsTypeFamily.Class;
			}

			return TsTypeFamily.Type;
		}

		/// <summary>
		/// Gets type of items in generic version of IEnumerable.
		/// </summary>
		/// <param name="type">The IEnumerable type to get items type from</param>
		/// <returns>The type of items in the generic IEnumerable or null if the type doesn't implement the generic version of IEnumerable.</returns>
		internal static Type GetEnumerableType(Type type) {
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
				return type.GetGenericArguments()[0];
			}

			foreach (Type intType in type.GetInterfaces()) {
				if (intType.IsGenericType && intType.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
					return intType.GetGenericArguments()[0];
				}
			}
			return null;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeLite.Extensions;

namespace TypeLite.TsModels {
	/// <summary>
	/// Represents a sysstem type in the code model.
	/// </summary>
	public class TsSystemType : TsType {
		/// <summary>
		/// Gets kind of the system type.
		/// </summary>
		public SystemTypeKind Kind { get; private set; }

		/// <summary>
		/// Initializes a new instance of the TsSystemType with the specific CLR type.
		/// </summary>
		/// <param name="clrType">The CLR type represented by this instance of the TsSystemType.</param>
		public TsSystemType(Type clrType)
			: base(clrType) {

			switch (this.ClrType.Name) {
				case "Boolean": this.Kind = SystemTypeKind.Bool; break;
				case "String":
				case "Char":
					this.Kind = SystemTypeKind.String; break;
				case "Byte":
				case "Int16":
				case "Int32":
				case "Int64":
				case "UInt16":
				case "UInt32":
				case "UInt64":
				case "Single":
				case "Double":
				case "Decimal":
					this.Kind = SystemTypeKind.Number; break;
				case "DateTime":
					this.Kind = SystemTypeKind.Date; break;
				default:
					throw new ArgumentException(string.Format("The type '{0}' is not supported system type.", clrType.FullName));
			}
		}
	}
}

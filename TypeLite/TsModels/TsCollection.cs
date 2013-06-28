using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeLite.TsModels {
	/// <summary>
	/// Represents a collection in the code model.
	/// </summary>
	public class TsCollection : TsType {
		/// <summary>
		/// Gets or sets type of the items in the collection.
		/// </summary>
		/// <remarks>
		/// If the collection isn't stronlgy typed, the ItemsType property is initialized to TsType.Any.
		/// </remarks>
		public TsType ItemsType { get; set; }

		/// <summary>
		/// Initializes a new instance of the TsCollection class with the specific CLR type.
		/// </summary>
		/// <param name="clrType">The CLR collection represented by this instance of the TsCollection.</param>
		public TsCollection(Type clrType)
			: base(clrType) {
			var enumerableType = TsType.GetEnumerableType(this.ClrType);
			if (enumerableType != null) {
				this.ItemsType = new TsType(enumerableType);
			} else if (typeof(IEnumerable).IsAssignableFrom(this.ClrType)) {
				this.ItemsType = TsType.Any;
			} else {
				throw new ArgumentException(string.Format("The type '{0}' is not collection.", this.ClrType.FullName));
			}
		}
	}
}

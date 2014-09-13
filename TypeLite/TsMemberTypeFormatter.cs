using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeLite.TsModels;

namespace TypeLite {
	/// <summary>
	/// Defines a method used to format class member types.
	/// </summary>
    /// <param name="memberTypeName">The type name to format</param>
    /// <param name="isMemberCollection">Indicates if member is collection</param>
	/// <returns>The formatted type.</returns>
	public delegate string TsMemberTypeFormatter(string memberTypeName, bool isMemberCollection, int collectionDimension = 1);
}

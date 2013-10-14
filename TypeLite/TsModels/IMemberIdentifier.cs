using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeLite.TsModels {
	/// <summary>
	/// Represents an identifier of a class member.
	/// </summary>
	public interface IMemberIdentifier {
		/// <summary>
		/// Gets name of the class member.
		/// </summary>
		string Name { get; }
	}
}

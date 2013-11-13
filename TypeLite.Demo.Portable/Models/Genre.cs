using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeLite.Demo.Portable.Models {
	[TsEnum(Module = "Library")]
	public enum Genre {
		Scifi = 1,
        Coursebook = 2
	}
}
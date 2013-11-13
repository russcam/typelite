using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeLite.Demo.Portable.Models {
	[TsClass(Module = "Library")]
	public class Library {
        public string Name { get; set; }

        public IEnumerable<Book> Books { get; set; }
	}
}
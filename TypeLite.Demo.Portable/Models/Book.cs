using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeLite.Demo.Portable.Models {
	[TsClass(Module = "Library")]
	public class Book {
		[TsIgnore]
		public int ID { get; set; }

        [TsProperty(Name="Title")]
		public string Name { get; set; }
		public int Pages { get; set; }

		public Genre Genre { get; set; }
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TypeLite.Demo.Models {
	[TsClass(Module = "Eshop")]
	public class Product {
		public string Name { get; set; }
		public decimal Price { get; set; }
		public Guid ID { get; set; }
	}
}
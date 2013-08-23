using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TypeLite.Demo.Models {
	[TsClass(Module = "Eshop")]
	public class Customer {
		[TsIgnore]
		public int ID { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }

		[TsProperty(Name = "VIP")]
		public bool IsVIP { get; set; }

		public CustomerKind Kind { get; set; }

		public IEnumerable<Order> Orders { get; set; }
	}
}
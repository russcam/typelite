using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeLite.Tests.TestModels {
	public class Address {
        public Guid Id { get; set; }
        public Guid[] Ids { get; set; }
		public string Street { get; set; }
		public string Town { get; set; }
		public ContactType AddressType { get; set; }
        public ConsoleKey Shortkey { get; set; }
	}
}

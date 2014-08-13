using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeLite.Tests.TestModels {
	public class Person {
		public string Name { get; set; }
		public int YearOfBirth { get; set; }

        // field
	    public string PhoneNumber;

		public Address PrimaryAddress { get; set; }
		public List<Address> Addresses { get; set; }
	}
}

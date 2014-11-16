using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TypeLite.Tests.RegressionTests {
    public class Issue56_JObject {

        [Fact]
        public void WhenPropertyOfJObjectIsUsed_ItIsGeneratedAsAny() {
            var builder = new TsModelBuilder();
            builder.Add<TestClassWithJObject>();

            var generator = new TsGenerator();
            var model = builder.Build();
            var result = generator.Generate(model);

            Assert.Contains("MyJObjectProperty: any;", result);
        }

        public class TestClassWithJObject {
            public JObject MyJObjectProperty { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TypeLite.Tests.RegressionTests {
    public class Issue43_EnumsWithDeclare {
        [Fact]
        public void WhenModuleIsGeneratedWithEnumsOnlyOption_ModuleDoesntHaveDeclareKeyword() {
            var builder = new TsModelBuilder();
            builder.Add<MyTestClass>();

            var generator = new TsGenerator();
            var model = builder.Build();
            var result = generator.Generate(model, TsGeneratorOutput.Enums);

            Assert.DoesNotContain("declare", result);
        }

        [Fact]
        public void WhenModuleIsGeneratedWithClassOnlyOption_ModuleHasDeclareKeyword() {
            var builder = new TsModelBuilder();
            builder.Add<MyTestClass>();

            var generator = new TsGenerator();
            var model = builder.Build();
            var result = generator.Generate(model, TsGeneratorOutput.Classes);

            Assert.Contains("declare", result);
        }

        public class MyTestClass {
            public int ID { get; set; }
            public MyTestEnum Enum { get; set; }
        }

        public enum MyTestEnum {
            One,
            Two,
            Three
        }
    }
}

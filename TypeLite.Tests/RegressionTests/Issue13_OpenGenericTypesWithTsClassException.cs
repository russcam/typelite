using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TypeLite.Tests.RegressionTests
{
    public class Issue13_OpenGenericTypesWithTsClassException
    {
        [Fact]
        public void WhenTsClassAppliedToOpenGenericType_NullReferenceExceptionShouldNotBeThrown()
        {
            var builder = new TsModelBuilder();
            builder.Add(typeof(A<>));

            var generator = new TsGenerator();
            var model = builder.Build();
            var result = generator.Generate(model);
        }

        [TsClass]
        public class A<T>
        {
            public T B { get; set; }
        }
    }
}

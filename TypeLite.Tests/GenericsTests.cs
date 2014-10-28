using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TypeLite.Tests;
using TypeLite.Tests.TestModels;
using Xunit;

namespace TypeLite.Tests
{
    public class GenericsTests
    {
        [Fact]
        public void CanGenerateSpecificTypesForGenericProperties()
        {
            var builder = new TsModelBuilder();
            builder.Add<GenerateSpecifyGenericTypesTestClass>();
            var model = builder.Build();
            var typeScript = new TsGenerator().Generate(model);
            Console.WriteLine(typeScript);

            var kvpClass = model.Classes.Single(c => !c.IsIgnored && c.Name == "KeyValuePair");
            Assert.NotNull(kvpClass);
            Assert.Equal("TKey", kvpClass.Properties.Single(p => p.Name == "Key").PropertyType.ClrType.Name);
            Assert.Equal("TValue", kvpClass.Properties.Single(p => p.Name == "Value").PropertyType.ClrType.Name);

            var classDefinition = model.Classes.Single(c => c.Name == typeof(GenerateSpecifyGenericTypesTestClass).Name);
            Assert.NotNull(classDefinition);
            Assert.True(((PropertyInfo)classDefinition.Properties.Single(p => p.Name == "StringToInt").ClrProperty).PropertyType == typeof(KeyValuePair<string, int>));

            Assert.Contains("StringToInt: System.Collections.Generic.KeyValuePair<string, number>;", typeScript);
        }

        [Fact]
        public void CanGenerateSpecificTypesForCollectionsOfGenericClasses()
        {
            var builder = new TsModelBuilder();
            builder.Add<GenerateSpecificCollectionsOfGenericTypesTestClass>();
            var model = builder.Build();
            var typeScript = new TsGenerator().Generate(model);
            Console.WriteLine(typeScript);

            Assert.Contains("ListOfIntToString: System.Collections.Generic.KeyValuePair<number,string>[];", typeScript);
        }


        [Fact]
        public void CanGenerateNestedGenericPropertiesForSystemTypes()
        {
            var builder = new TsModelBuilder();
            builder.Add<HandleNestedGenericsSystemTypesTestClass>();
            var model = builder.Build();

            var generator = new TsGenerator();
            var typeScript = generator.Generate(model);

            Assert.Contains("NestedStringList: string[][]", typeScript);
        }


        [Fact(Skip = "Currently produces any[][]")]
        public void CanGenerateNestedGenericPropertiesForCustomTypes()
        {
            var builder = new TsModelBuilder();
            builder.Add<HandleNestedGenericsCustomTypesTestClass>();
            var model = builder.Build();

            var generator = new TsGenerator();
            var typeScript = generator.Generate(model);
            Console.WriteLine(typeScript);

            Assert.Contains("NestedStringList: TypeLite.Tests.GenericsTests.GenerateSpecifyGenericTypesTestClass[][]", typeScript);
        }

        [Fact]
        public void CanHandleGenericArgsInBaseClass()
        {
            var builder = new TsModelBuilder();
            builder.Add<DerivedGenericClass>();
            var model = builder.Build();

            var generator = new TsGenerator();
            var typeScript = generator.Generate(model);
            Console.WriteLine(typeScript);

            Assert.Contains("SomeGenericProperty: TType;", typeScript);
            Assert.Contains("SomeGenericArrayProperty: TType[];", typeScript);
            Assert.Contains("interface DerivedGenericClass extends TypeLite.Tests.GenericsTests.BaseGeneric<string>", typeScript);
        }

        [Fact]
        public void GenericParameterTypeIsFullyQualified()
        {
            var builder = new TsModelBuilder();
            builder.Add<DerivedGenericClassWithArgInDifferentNamespace>();
            var model = builder.Build();

            var generator = new TsGenerator();
            var typeScript = generator.Generate(model);
            Console.WriteLine(typeScript);

            Assert.Contains("interface DerivedGenericClassWithArgInDifferentNamespace extends TypeLite.Tests.GenericsTests.BaseGeneric<DummyNamespace.Test>", typeScript);
        }

        private class HandleNestedGenericsSystemTypesTestClass
        {
            public List<List<string>> NestedStringList { get; set; }
        }

        private class HandleNestedGenericsCustomTypesTestClass
        {
            public List<List<GenerateSpecifyGenericTypesTestClass>> NestedCustomClassList { get; set; }
        }

        private class GenerateSpecificCollectionsOfGenericTypesTestClass
        {
            public List<KeyValuePair<int, string>> ListOfIntToString { get; set; }
        }

        private class GenerateSpecifyGenericTypesTestClass
        {
            public KeyValuePair<string, int> StringToInt { get; set; }
        }

        internal class BaseGeneric<TType>
        {
            public TType SomeGenericProperty { get; set; }
            public TType[] SomeGenericArrayProperty { get; set; }
        }

        private class DerivedGenericClass : BaseGeneric<string>
        {
        }

        private class DerivedGenericClassWithArgInDifferentNamespace : BaseGeneric<DummyNamespace.Test>
        {
        }
    }
}

namespace DummyNamespace
{
    public class Test
    {
    }
}
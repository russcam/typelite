using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeLite.Tests.TestModels;
using TypeLite.TsModels;
using Xunit;

namespace TypeLite.Tests.TsModels {
	public class TsClassTests {

		[Fact]
		public void WhenInitialized_NameIsSet() {
			var target = new TsClass(typeof(Person));

			Assert.Equal("Person", target.Name);
		}

		[Fact]
		public void WhenInitialized_IsIgnoredIsFalse() {
			var target = new TsClass(typeof(Person));

			Assert.False(target.IsIgnored);
		}

		[Fact]
		public void WhenInitialized_PropertiesAreCreated() {
			var target = new TsClass(typeof(Address));

			Assert.Single(target.Properties.Where(o => o.ClrProperty == typeof(Address).GetProperty("Street")));
			Assert.Single(target.Properties.Where(o => o.ClrProperty == typeof(Address).GetProperty("Town")));
		}

		[Fact]
		public void WhenInitializedWithClassWithBaseTypeObject_BaseTypeIsSetToNull() {
			var target = new TsClass(typeof(Address));

			Assert.Null(target.BaseType);
		}

		[Fact]
		public void WhenInitializedWithClassThatHasBaseClass_BaseTypeIsSet() {
			var target = new TsClass(typeof(Employee));

			Assert.NotNull(target.BaseType);
			Assert.Equal(typeof(Person), target.BaseType.ClrType);
		}

		[Fact]
		public void WhenInitializedWithClassThatHasBaseClass_OnlyPropertiesDefinedInDerivedClassAreCreated() {
			var target = new TsClass(typeof(Employee));

			Assert.Single(target.Properties.Where(o => o.ClrProperty == typeof(Employee).GetProperty("Salary")));

			Assert.Empty(target.Properties.Where(o => o.ClrProperty == typeof(Employee).GetProperty("Street")));
			Assert.Empty(target.Properties.Where(o => o.ClrProperty == typeof(Employee).GetProperty("Street")));
		}

		[Fact]
		public void WhenInitializedAndClassHasCustomNameInAttribute_CustomNameIsUsed() {
			var target = new TsClass(typeof(CustomClassName));

			Assert.Equal("MyClass", target.Name);
		}

		[Fact]
		public void WhenInitialized_ModuleIsSetToNamespaceModule() {
			var target = new TsClass(typeof(Address));

			Assert.NotNull(target.Module);
			Assert.Equal(typeof(Address).Namespace, target.Module.Name);
		}

		[Fact]
		public void WhenInitializedAndClassHasCustomModuleInAttribute_CustomModuleIsUsed() {
			var target = new TsClass(typeof(CustomClassName));

			Assert.Equal("MyModule", target.Module.Name);
		}

		#region Module property tests

		[Fact]
		public void WhenModuleIsSet_ClassIsAddedToModule() {
			var module = new TsModule("Tests");
			var target = new TsClass(typeof(Address));

			target.Module = module;

			Assert.Contains(target, module.Classes);
		}

		[Fact]
		public void WhenModuleIsSetToOtherModule_ClassIsRemovedFromOriginalModule() {
			var originalModule = new TsModule("Tests.Original");
			var module = new TsModule("Tests");
			var target = new TsClass(typeof(Address));

			target.Module = originalModule;
			target.Module = module;

			Assert.DoesNotContain(target, originalModule.Classes);
		}

		#endregion
	}
}

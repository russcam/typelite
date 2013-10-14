using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using TypeLite.TsModels;

namespace TypeLite {
	/// <summary>
	/// Provides helper methods for generating TypeScript definition files.
	/// </summary>
	public static class TypeScript {
		/// <summary>
		/// Creates an instance of the FluentTsModelBuider for use in T4 templates.
		/// </summary>
		/// <returns>An instance of the FluentTsModelBuider</returns>
		public static TypeScriptFluent Definitions() {
			return new TypeScriptFluent();
		}
	}

	/// <summary>
	/// Represents a wrapper around TsModelBuilder and TsGenerator that simplify usage a enables fluent configuration.
	/// </summary>
	public class TypeScriptFluent {
		protected TsModelBuilder _modelBuilder;
		protected TsGenerator _scriptGenerator;

		/// <summary>
		/// Gets the ModelBuilder being configured with fluent configuration.
		/// </summary>
		public TsModelBuilder ModelBuilder {
			get {
				return _modelBuilder;
			}
		}

		/// <summary>
		/// Initializes a new instance of the TypeScriptFluent class
		/// </summary>
		public TypeScriptFluent() {
			_modelBuilder = new TsModelBuilder();
			_scriptGenerator = new TsGenerator();
		}

		/// <summary>
		/// Initializes a new instance of the TypeScriptFluent class
		/// </summary>
		/// <param name="fluentConfigurator">The source fluent configurator</param>
		protected TypeScriptFluent(TypeScriptFluent fluentConfigurator) {
			_modelBuilder = fluentConfigurator._modelBuilder;
			_scriptGenerator = fluentConfigurator._scriptGenerator;
		}

		/// <summary>
		/// Adds specific class with all referenced classes to the model.
		/// </summary>
		/// <typeparam name="T">The class type to add.</typeparam>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		public TypeScriptFluentClass For<T>() {
			return this.For(typeof(T));
		}

		/// <summary>
		/// Adds specific class with all referenced classes to the model.
		/// </summary>
		/// <param name="type">The type to add to the model.</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		public TypeScriptFluentClass For(Type type) {
			var classModel = _modelBuilder.Add(type);
			return new TypeScriptFluentClass(this, classModel);
		}

		/// <summary>
		/// Adds all classes annotated with the TsClassAttribute from an assembly to the model.
		/// </summary>
		/// <param name="assembly">The assembly with classes to add.</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		public TypeScriptFluent For(Assembly assembly) {
			_modelBuilder.Add(assembly);
			return this;
		}

		/// <summary>
		/// Registers a formatter for the specific type
		/// </summary>
		/// <typeparam name="TFor">The type to register the formatter for. TFor is restricted to TsType and derived classes.</typeparam>
		/// <param name="formatter">The formatter to register</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		public TypeScriptFluent WithFormatter<TFor>(TsTypeFormatter formatter) where TFor : TsType {
			_scriptGenerator.RegisterTypeFormatter<TFor>(formatter);
			return this;
		}

		/// <summary>
		/// Registers a formatter for the the TsClass type.
		/// </summary>
		/// <param name="formatter">The formatter to register</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		public TypeScriptFluent WithFormatter(TsTypeFormatter formatter) {
			_scriptGenerator.RegisterTypeFormatter(formatter);
			return this;
		}

		/// <summary>
		/// Registers a formatter for member identifiers
		/// </summary>
		/// <param name="formatter">The formatter to register</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		public TypeScriptFluent WithFormatter(TsMemberIdentifierFormatter formatter) {
			_scriptGenerator.RegisterIdentifierFormatter(formatter);
			return this;
		}

		/// <summary>
		/// Registers a converter for the specific type
		/// </summary>
		/// <typeparam name="TFor">The type to register the converter for.</typeparam>
		/// <param name="convertor">The converter to register</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		public TypeScriptFluent WithConvertor<TFor>(TypeConvertor convertor) {
			_scriptGenerator.RegisterTypeConvertor<TFor>(convertor);
			return this;
		}

		/// <summary>
		/// Generates TypeScript definitions for types included in this model builder.
		/// </summary>
		/// <returns>TypeScript definition for types included in this model builder.</returns>
		public string Generate() {
			var model = _modelBuilder.Build();
			return _scriptGenerator.Generate(model);
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>TypeScript definition for types included in this model builder.</returns>
		public override string ToString() {
			return this.Generate();
		}
	}

	/// <summary>
	/// Represents a wrapper around TsModelBuilder and TsGenerator that simplify usage a enables fluent configuration for classes.
	/// </summary>
	public class TypeScriptFluentClass : TypeScriptFluent {
		public TsClass Class { get; protected set; }

		internal TypeScriptFluentClass(TypeScriptFluent fluentConfigurator, TsClass classModel)
			: base(fluentConfigurator) {
			this.Class = classModel;
		}

		/// <summary>
		/// Changes the name of the class being configured .
		/// </summary>
		/// <param name="name">The new name of the class</param>
		/// <returns>Instance of the TypeScriptFluentClass that enables fluent configuration.</returns>
		public TypeScriptFluentClass Named(string name) {
			this.Class.Name = name;
			return this;
		}

		/// <summary>
		/// Maps the class being configured to the specific module
		/// </summary>
		/// <param name="moduleName">The name of the module</param>
		/// <returns>Instance of the TypeScriptFluentClass that enables fluent configuration.</returns>
		public TypeScriptFluentClass ToModule(string moduleName) {
			this.Class.Module = new TsModule(moduleName);
			return this;
		}
	}
}

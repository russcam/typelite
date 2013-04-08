using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeLite.TsModels;

namespace TypeLite {
	/// <summary>
	/// Resolves TsTypes to more specialized types 
	/// </summary>
	/// <remarks>
	/// When a class is added to the model by TsModelBuilder, TsType is used for all type references. The purpose of the TypeResolver is to visit references and resolve them to the more specific types.
	/// </remarks>
	internal class TypeResolver : TsModelVisitor {
		TsModel _model;
		Dictionary<Type, TsType> _knownTypes;
		Dictionary<string, TsModule> _modules;

		/// <summary>
		/// Initializes a new instance of the TypeResolver.
		/// </summary>
		/// <param name="model">The model to process.</param>
		public TypeResolver(TsModel model) {
			_model = model;
			_modules = new Dictionary<string, TsModule>();
			_knownTypes = new Dictionary<Type, TsType>();

			foreach (var classModel in model.Classes) {
				_knownTypes[classModel.ClrType] = classModel;
			}
		}

		/// <summary>
		/// Resolves references in the class.
		/// </summary>
		/// <param name="classModel"></param>
		public override void VisitClass(TsClass classModel) {
			if (classModel.Module != null) {
				classModel.Module = this.ResolveModule(classModel.Module.Name);
			}

			if (classModel.BaseType != null && classModel.BaseType != TsType.Any) {
				classModel.BaseType = this.ResolveType(classModel.BaseType);
			}
		}

		/// <summary>
		/// Resolves references in the property.
		/// </summary>
		/// <param name="property"></param>
		public override void VisitProperty(TsProperty property) {
			property.PropertyType = this.ResolveType(property.PropertyType);
		}

		/// <summary>
		/// Resolves TsType to the more specialized type.
		/// </summary>
		/// <param name="toResolve">The type to resolve.</param>
		/// <returns></returns>
		private TsType ResolveType(TsType toResolve) {
			if (!(toResolve is TsType)) {
				return toResolve;
			}

			if (_knownTypes.ContainsKey(toResolve.ClrType)) {
				return _knownTypes[toResolve.ClrType];
			}

			var typeFamily = TsType.GetTypeFamily(toResolve.ClrType);
			TsType type = null;

			switch (typeFamily) {
				case TsTypeFamily.System: type = new TsSystemType(toResolve.ClrType); break;
				case TsTypeFamily.Collection: type = this.CreateCollectionType(toResolve); break;
				default: type = TsType.Any; break;
			}

			_knownTypes[toResolve.ClrType] = type;
			return type;
		}

		/// <summary>
		/// Creates a TsCollection from TsType
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		private TsCollection CreateCollectionType(TsType type) {
			var resolved = new TsCollection(type.ClrType);
			resolved.ItemsType = this.ResolveType(resolved.ItemsType);
			return resolved;
		}

		/// <summary>
		/// Resolves module instance from the module name.
		/// </summary>
		/// <param name="name">The name of the module</param>
		/// <returns></returns>
		private TsModule ResolveModule(string name) {
			if (_modules.ContainsKey(name)) {
				return _modules[name];
			}

			var module = new TsModule(name);
			_modules[name] = module;
			_model.Modules.Add(module);
			return module;
		}
	}
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TypeLite.Extensions;
using TypeLite.TsModels;

namespace TypeLite {
	/// <summary>
	/// Creates a script model from CLR classes.
	/// </summary>
	public class TsModelBuilder {
		/// <summary>
		/// Gets or sets collection of classes in the model being built.
		/// </summary>
		internal Dictionary<Type, TsClass> Classes { get; set; }
		internal Dictionary<Type, TsEnum> Enums { get; set; }

		/// <summary>
		/// Initializes a new instance of the TsModelBuilder class.
		/// </summary>
		public TsModelBuilder() {
			this.Classes = new Dictionary<Type, TsClass>();
			this.Enums = new Dictionary<Type, TsEnum>();
		}

		/// <summary>
		/// Adds type with all referenced classes to the model.
		/// </summary>
		/// <typeparam name="T">The type to add to the model.</typeparam>
		/// <returns>type added to the model</returns>
		public TsClass Add<T>() {
			return this.Add<T>(true);
		}

		/// <summary>
		/// Adds type and optionally referenced classes to the model.
		/// </summary>
		/// <typeparam name="T">The type to add to the model.</typeparam>
		/// <param name="includeReferences">bool value indicating whether classes referenced by T should be added to the model.</param>
		/// <returns>type added to the model</returns>
		public TsClass Add<T>(bool includeReferences) {
			return this.Add(typeof(T), includeReferences);
		}

		/// <summary>
		/// Adds type with all referenced classes to the model.
		/// </summary>
		/// <param name="clrType">The type to add to the model.</param>
		/// <returns>type added to the model</returns>
		public TsClass Add(Type clrType) {
			return this.Add(clrType, true);
		}

		/// <summary>
		/// Adds type and optionally referenced classes to the model.
		/// </summary>
		/// <param name="clrType">The type to add to the model.</param>
		/// <param name="includeReferences">bool value indicating whether classes referenced by T should be added to the model.</param>
		/// <returns>type added to the model</returns>
		public TsClass Add(Type clrType, bool includeReferences) {
			var typeFamily = TsType.GetTypeFamily(clrType);
			if (typeFamily != TsTypeFamily.Class) {
				throw new ArgumentException(string.Format("Type '{0}' isn't class or struct. Only classes and structures can be added to the model", clrType.FullName));
			}

			if (clrType.IsNullable()) {
				return this.Add(clrType.GetNullableValueType(), includeReferences);
			}

			if (!this.Classes.ContainsKey(clrType)) {
				var added = new TsClass(clrType);
				this.Classes[clrType] = added;

				if (added.BaseType != null) {
					this.Add(added.BaseType.ClrType);
				}
				if (includeReferences) {
					this.AddReferences(added);

					foreach (var e in added.Properties.Where(p => p.PropertyType.ClrType.IsEnum))
						this.AddEnum(e.PropertyType as TsEnum);
				}

				return added;
			} else {
				return this.Classes[clrType];
			}
		}

		/// <summary>
		/// Adds enum to the model
		/// </summary>
		/// <param name="tsEnum">The enum to add</param>
		private void AddEnum(TsEnum tsEnum) {
			if (!this.Enums.ContainsKey(tsEnum.ClrType)) {
				this.Enums[tsEnum.ClrType] = tsEnum;
			}
		}

		/// <summary>
		/// Adds all classes annotated with the TsClassAttribute from an assembly to the model.
		/// </summary>
		/// <param name="assembly">The assembly with classes to add</param>
		public void Add(Assembly assembly) {
			foreach (var type in assembly.GetTypes().Where(t => t.GetCustomAttribute<TsClassAttribute>(false) != null && TsType.GetTypeFamily(t) == TsTypeFamily.Class)) {
				this.Add(type);
			}
		}

		/// <summary>
		/// Build the model.
		/// </summary>
		/// <returns>The script model with the classes.</returns>
		public TsModel Build() {
			var model = new TsModel(this.Classes.Values, this.Enums.Values);
			model.RunVisitor(new TypeResolver(model));
			return model;
		}

		/// <summary>
		/// Adds classes referenced by the class to the model
		/// </summary>
		/// <param name="classModel"></param>
		private void AddReferences(TsClass classModel) {
			foreach (var property in classModel.Properties.Where(model => !model.IsIgnored)) {
				var propertyTypeFamily = TsType.GetTypeFamily(property.PropertyType.ClrType);
				if (propertyTypeFamily == TsTypeFamily.Collection) {
					var collectionItemType = TsType.GetEnumerableType(property.PropertyType.ClrType);
					if (collectionItemType != null) {
						var typeFamily = TsType.GetTypeFamily(collectionItemType);

						switch (typeFamily){
							case TsTypeFamily.Class:
								this.Add(collectionItemType);
								break;
							case TsTypeFamily.Enum:
								this.AddEnum(new TsEnum(collectionItemType));
								break;
						}
					}
				} else if (propertyTypeFamily == TsTypeFamily.Class) {
					this.Add(property.PropertyType.ClrType);
				}
			}
		}
	}


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeLite.TsModels;

namespace TypeLite {
	/// <summary>
	/// Contains a collection of convertors that provides a way to convert the specific type to custom string bypassing other rules.
	/// </summary>
	public class TypeConvertorCollection {
		internal Dictionary<Type, TypeConvertor> _convertors;

		/// <summary>
		/// Initializes a new instance of the TypeConvertorCollection class.
		/// </summary>
		public TypeConvertorCollection() {
			_convertors = new Dictionary<Type, TypeConvertor>();
		}

		/// <summary>
		/// Registers the convertor for the specific Type
		/// </summary>
		/// <typeparam name="TFor">The type to register the convertor for.</typeparam>
		/// <param name="convertor">The convertor to register</param>
		public void RegisterTypeConverter<TFor>(TypeConvertor convertor) {
			_convertors[typeof(TFor)] = convertor;
		}

		/// <summary>
		/// Checkes whether any convertor is registerd for the specific Type
		/// </summary>
		/// <param name="type">The type to check</typeparam>
		/// <returns>true if a convertor is registered for the specific Type otherwise return false</returns>
		public bool IsConvertorRegistered(Type type) {
			return _convertors.ContainsKey(type);
		}

		/// <summary>
		/// Converts specific type to its string representation.
		/// </summary>
		/// <param name="type">The type to convert</typeparam>
		/// <returns>the string representation of the type if a convertor fo the type is registered otherwise return null</returns>
		public string ConvertType(Type type) {
			if (_convertors.ContainsKey(type)) {
				return _convertors[type](type);
			}

			return null;
		}
	}
}

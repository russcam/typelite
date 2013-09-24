using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeLite.TsModels {
	/// <summary>
	/// Represents a type that can be places inside module
	/// </summary>
	public abstract class TsModuleMember : TsType {
		private TsModule _module;

		/// <summary>
		/// Gets or sets module, that contains this class.
		/// </summary>
		public TsModule Module {
			get {
				return _module;
			}
			set {
				if (_module != null) {
					_module.Remove(this);
				}
				_module = value;
				if (_module != null) {
					_module.Add(this);
				}
			}
		}

		/// <summary>
		/// Initializes TsModuleMember class with the specific CLR type.
		/// </summary>
		/// <param name="clrType">The CLR type represented by this instance of the ModuleMember</param>
		public TsModuleMember(Type clrType)
			: base(clrType) {

			this.Module = new TsModule(this.ClrType.Namespace);
		}
	}
}

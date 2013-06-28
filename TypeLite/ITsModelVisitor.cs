using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeLite.TsModels;

namespace TypeLite {
	/// <summary>
	/// Defines an interface of TypeScript model visitor, that can be used to examine and modify TypeScript model.
	/// </summary>
	public interface ITsModelVisitor {
        /// <summary>
        /// Represents a method called once for the model.
        /// </summary>
        /// <param name="model">The model being visited.</param>
        void VisitModel(TsModel model);

		/// <summary>
		/// Represents a method called for every module in the model.
		/// </summary>
		/// <param name="module">The module being visited.</param>
		void VisitModule(TsModule module);

		/// <summary>
		/// Represents a method called for every class in the model.
		/// </summary>
		/// <param name="classModel">The model class being visited.</param>
		void VisitClass(TsClass classModel);

		/// <summary>
		/// Represents a method called for every property in the model.
		/// </summary>
		/// <param name="property">The property being visited.</param>
		void VisitProperty(TsProperty property);
	}
}

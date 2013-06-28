using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeLite.TsModels;

namespace TypeLite {
    /// <summary>
    /// Represents script model of CLR classes.
    /// </summary>
    public class TsModel {
        /// <summary>
        /// Gets a collection of classes in the model.
        /// </summary>
        public ISet<TsClass> Classes { get; private set; }

        /// <summary>
        /// Gets a collection of references to other d.ts files.
        /// </summary>
        public ISet<string> References { get; private set; }

		/// <summary>
		/// Gets a collection of modules in the module.
		/// </summary>
		public ISet<TsModule> Modules { get; private set; }

        /// <summary>
        /// Initializes a new instance of the TsModel class.
        /// </summary>
        public TsModel()
            : this(new TsClass[] { }) {
        }

        /// <summary>
        /// Initializes a new instance of the TsModel class with collection of classes.
        /// </summary>
        /// <param name="classes">The collection of classes to add to the model.</param>
        public TsModel(IEnumerable<TsClass> classes) {
            this.Classes = new HashSet<TsClass>(classes);
            this.References = new HashSet<string>();
			this.Modules = new HashSet<TsModule>();
        }

        /// <summary>
        /// Runs specific model visitor.
        /// </summary>
        /// <param name="visitor">The model visitor to run.</param>
        public void RunVisitor(TsModelVisitor visitor) {
			visitor.VisitModel(this);

			foreach (var module in this.Modules) {
				visitor.VisitModule(module);
			}

            foreach (var classModel in this.Classes) {
                visitor.VisitClass(classModel);

                foreach (var property in classModel.Properties) {
                    visitor.VisitProperty(property);
                }
            }
        }
    }
}

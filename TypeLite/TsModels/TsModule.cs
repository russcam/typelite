using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeLite.TsModels {
    /// <summary>
    /// Represents a module in the script model.
    /// </summary>
    public class TsModule {
        private ISet<TsClass> _classes;

        /// <summary>
        /// Gets or sets name of the module.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets collection of classes in the module.
        /// </summary>
        public IEnumerable<TsClass> Classes {
            get {
                return _classes;
            }
        }

        /// <summary>
        /// Initializes a new instance of the TsModule class.
        /// </summary>
        public TsModule(string name) {
            _classes = new HashSet<TsClass>();
            this.Name = name;
        }

        /// <summary>
        /// Adds class to this module.
        /// </summary>
        /// <param name="toAdd">The class to add.</param>
        internal void AddClass(TsClass toAdd) {
            _classes.Add(toAdd);
        }

        /// <summary>
        /// Removes class from this module.
        /// </summary>
        /// <param name="toRemove">The class to remove.</param>
        internal void RemoveClass(TsClass toRemove) {
            _classes.Remove(toRemove);
        }
    }
}

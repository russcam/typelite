using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeLite.TsModels;

namespace TypeLite {
    /// <summary>
    /// Defines a method used to format the name of a module.
    /// </summary>
    /// <param name="typeName">The type name to format</param>
    /// <returns>A bool indicating if a member should be exported.</returns>
    public delegate string TsModuleNameFormatter(string moduleName);
}

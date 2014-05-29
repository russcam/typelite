using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeLite {
    /// <summary>
    /// Defines output of the generator
    /// </summary>
    [Flags]
    public enum TsGeneratorOutput {
        Classes = 1,
        Enums = 2
    }
}

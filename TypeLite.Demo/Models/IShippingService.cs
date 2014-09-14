using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TypeLite.Demo.Models {
    [TsInterface]
    public interface IShippingService {
        double Price { get; set; }
    }
}
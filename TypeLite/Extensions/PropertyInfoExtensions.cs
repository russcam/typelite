using System;
using System.Linq;
using System.Reflection;

namespace TypeLite.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static TType GetCustomAttribute<TType>(this PropertyInfo propertyInfo, bool inherit) where TType : Attribute
        {
            return propertyInfo.GetCustomAttributes(typeof(TType), inherit).FirstOrDefault() as TType;
        }
    }
}

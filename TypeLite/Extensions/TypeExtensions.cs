using System;
using System.Linq;
using System.Reflection;

namespace TypeLite.Extensions
{
    public static class TypeExtensions
    {
        public static TType GetCustomAttribute<TType>(this Type type, bool inherit) where TType : Attribute
        {
            return type.GetCustomAttributes(typeof(TType), inherit).FirstOrDefault() as TType;
        }
    }
}

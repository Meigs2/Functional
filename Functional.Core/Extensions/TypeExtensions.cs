using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Functional.Core;

// ReSharper disable once CheckNamespace
namespace System
{
    internal static class TypeExtensions
    {
        public static List<TFieldType> GetFieldsOfType<TFieldType>(this Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(p => type.IsAssignableFrom(p.FieldType)).Select(pi => (TFieldType)pi.GetValue(null)).ToList();
        }
    }

    public static class ObjectExtensions
    {
        public static Type GetType(this object obj) { return obj.GetType(); }

        public static Option<T> CreateInstanceOf<T>(this Type type, params object[] args)
        {
            try { return (T)Activator.CreateInstance(typeof(T), args); }
            catch (Exception e) { return Option.None; }
        }
    }
}

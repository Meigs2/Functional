using System;

namespace Meigs2.Functional.Enumeration;

public abstract record DynamicEnumeration<TObject, TValue> : Enumeration<TObject, TValue>
    where TObject : DynamicEnumeration<TObject, TValue> where TValue : IEquatable<TValue>, IComparable<TValue>
{
    protected DynamicEnumeration(string name, TValue value) : base(name, value)
    {
    }

    protected static Option<TObject> AddValue(TObject newValue)
    {
        return AddNewValue(newValue);
    }
}
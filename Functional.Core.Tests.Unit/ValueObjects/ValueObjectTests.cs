using NUnit.Framework;

namespace Meigs2.Functional.Tests.Unit.ValueObjects;

public class ValueObjectTests
{
    [Test]
    public void ValueObjectTValue()
    {
        var value1 = Value.From(1);
        var value2 = Value.From(2);
        var value3 = Value.From(1);

        Assert.AreEqual(1, value1.Value);
        Assert.AreEqual(2, value2.Value);
        Assert.AreEqual(1, value3.Value);

        Assert.AreEqual(value1, value3);
        Assert.AreNotEqual(value1, value2);
        Assert.AreNotEqual(value2, value3);
    }
}


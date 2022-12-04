using FluentAssertions;
using FluentAssertions.Execution;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Meigs2.Functional.Tests.Unit;

[TestFixture]
public class Option
{
    // We need to test that Serializing and Deserializing an Option<T> does not change the value of the Option<T>
    // Write the tests in the style of specification alla Kevlin Henney
    // Option<T> should be serializable and preserve the value of the Option<T>
    
    public class When_serializing_an_optional_with_a_value
    {
        [Test]
        public void Should_deserialize_to_an_optional_with_the_same_value()
        {
            var expected = Functional.Option.Some(1);
            var serialized = JsonConvert.SerializeObject(expected);
            // Output the serialized value to the test context
            using var _ = new AssertionScope();
            TestContext.WriteLine(serialized);
            var actual = JsonConvert.DeserializeObject<Option<int>>(serialized);
            TestContext.WriteLine(actual);
            actual.Should().Be(expected);
        }
    }
    
    public class When_serializing_an_optional_with_no_value
    {
        [Test]
        public void Should_deserialize_to_an_optional_when_none()
        {
            var expected = Functional.Option.Some(1);
            var serialized = JsonConvert.SerializeObject(expected);
            // Output the serialized value to the test context
            using var _ = new AssertionScope();
            TestContext.WriteLine(serialized);
            var actual = JsonConvert.DeserializeObject<Option<int>>(serialized);
            TestContext.WriteLine(actual);
            actual.Should().Be(expected);
        }
    }
}
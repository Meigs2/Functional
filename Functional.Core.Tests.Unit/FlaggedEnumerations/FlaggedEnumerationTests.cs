using FluentAssertions;
using Functional.Core;
using NUnit.Framework;

namespace Functional.Tests.Unit.FlaggedEnumerations;

public class FlaggedEnumerationTests
{
    public record TestFlagEnum : FlaggedEnumeration<TestFlagEnum, int>
    {
        public static readonly TestFlagEnum One = new(nameof(One),1);
        public static readonly TestFlagEnum Two = new(nameof(Two),2);
        public static readonly TestFlagEnum OneAndTwo = new(nameof(OneAndTwo),One | Two);
        protected TestFlagEnum(string name, int value) : base(name, value)
        {
        }
    }

    [Test]
    public void SingleFlag()
    {
        TestFlagEnum.FromName("One").ValueOrThrow().ForEach(x => TestContext.WriteLine(x));
        TestFlagEnum.FromName("One").ValueOrThrow().Should().Contain(TestFlagEnum.One);
    }
    
    [Test]
    public void CombinedFlag()
    {
        TestFlagEnum.Values.ForEach(x => TestContext.WriteLine(x));
        TestFlagEnum.GetSingle(TestFlagEnum.OneAndTwo).ValueOrThrow().Should().Be(TestFlagEnum.OneAndTwo);
        TestFlagEnum.FromName("OneAndTwo").ValueOrThrow().ForEach(x => TestContext.WriteLine(x));
    }
}

public class FlaggedDynamicEnumerations
{
    
}

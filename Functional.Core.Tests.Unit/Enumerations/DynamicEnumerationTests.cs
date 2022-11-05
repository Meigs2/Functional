using Meigs2.Functional.Enumeration;
using NUnit.Framework;

namespace Meigs2.Functional.Tests.Unit.Enumerations;

public class DynamicEnumerationTests
{
    public record TestDynamicEnumeration : DynamicEnumeration<TestDynamicEnumeration, int>
    {
        public static readonly TestDynamicEnumeration One = new(nameof(One),1);
        public static readonly TestDynamicEnumeration Two = new(nameof(Two),2);
        public static readonly TestDynamicEnumeration Four = new(nameof(Four),4);
        
        public static void AddNew()
        {
            AddValue(new TestDynamicEnumeration("New", 8));
        }
        
        protected TestDynamicEnumeration(string name, int value) : base(name, value)
        {
        }
    }
    
    [OneTimeSetUp]
    public void Setup()
    {
        TestDynamicEnumeration.AddNew();
    }
    
    [Test]
    public void SingleFlag()
    {
        TestContext.WriteLine(TestDynamicEnumeration.FromName("New"));
        TestDynamicEnumeration.FromName("New").ValueOrThrow();
    }
}

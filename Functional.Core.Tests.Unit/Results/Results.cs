using Meigs2.Functional.Results;
using NUnit.Framework;
using FluentAssertions;
using FluentAssertions.Execution;

namespace Meigs2.Functional.Tests.Unit.Results;

[TestFixture]
public class Results
{
    // Write a test to ensure that when an exception is returned the result is a failure.
    // Write all tests in the style of Kevlin Henney's "The Art of Unit Testing" book.

    public class From_Errors
    {
        [Test]
        public void Should_Return_Failure()
        {
            var result = Result.Failure(new Exception("Test"));

            // use an assertion scope and FluentAssertions to write the test
            using (new AssertionScope())
            {
                result.IsSuccess.Should().BeFalse();
                result.IsFailure.Should().BeTrue();
                result.Error.Message.Should().Be("Test");
            }
        }
    }

    public class Exceptions
    {
        [Test]
        public void Try_Catch_Return_Should_Return_Failure_With_Error()
        {
            Result<string> result;
            try { throw new Exception("Test"); }
            catch (Exception e) { result = e; }

            // use an assertion scope and FluentAssertions to write the test
            using var scope = new AssertionScope();
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.Error.Message.Should().Be("Test");
        }
    }
}
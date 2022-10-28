using System.Diagnostics;
using FluentAssertions;
using Functional.Core;
using NUnit.Framework;

namespace Functional.Tests.Unit.AsyncOperations;

public class AsyncOperations
{
    public async Task<int> AsyncDelayedAdd(int a, int b, TimeSpan delay)
    {
        await Task.Delay(delay);
        return a + b;
    }

    [Test] public async Task Chained_Async_Calls_Are_All_Awaited()
    {
        var sw = new Stopwatch();
        sw.Start();
        var result = await AsyncDelayedAdd(1, 0, TimeSpan.FromSeconds(1))
                          .FromTask()
                          .MapAsync(x => AsyncDelayedAdd(x, 1, TimeSpan.FromSeconds(1)))
                          .MapAsync(x => AsyncDelayedAdd(x, 1, TimeSpan.FromSeconds(1)))
                          .Map(x => x + 5)
                          .GetTask();
        
        sw.Stop();
        sw.Elapsed.Should().BeGreaterThan(TimeSpan.FromSeconds(3));
        result.Should().Be(8);
    }
    
    [Test] public async Task Chained_Async_Calls_Are_All_Awaited_With_Match()
    {
        var sw = new Stopwatch();
        sw.Start();
        var result = await AsyncDelayedAdd(1, 0, TimeSpan.FromSeconds(1))
                          .FromTask()
                          .MapAsync(x => AsyncDelayedAdd(x, 1, TimeSpan.FromSeconds(1)))
                          .MapAsync(x => AsyncDelayedAdd(x, 1, TimeSpan.FromSeconds(1)))
                          .Map(x => x + 5)
                          .Match(predicate: i => i > 0,
                                 success: x => x,
                                 failure: x => throw new Exception("Should not be called")
                          )
                          .GetTask();
        sw.Stop();
        sw.Elapsed.Should().BeGreaterThan(TimeSpan.FromSeconds(3));
        result.Should().Be(8);
    }
    
    [Test] public async Task Catch_Should_Catch_Exceptions()
    {
        var sw = new Stopwatch();
        sw.Start();
        var result = await AsyncDelayedAdd(1, 0, TimeSpan.FromSeconds(1))
                          .FromTask()
                          .MapAsync(x => AsyncDelayedAdd(x, 1, TimeSpan.FromSeconds(1)))
                          .MapAsync(x => AsyncDelayedAdd(x, 1, TimeSpan.FromSeconds(1)))
                          .Map<int>(x => throw new Exception())
                          .Map(x => x + 5)
                          .Catch(ex => 0)
                          .GetTask();
        sw.Stop();
        sw.Elapsed.Should().BeGreaterThan(TimeSpan.FromSeconds(3));
        result.Should().Be(0);
    }
    
    [Test] public async Task Catch_Should_Catch_Exceptions_With_Match()
    {
        var sw = new Stopwatch();
        sw.Start();
        var result = await AsyncDelayedAdd(1, 0, TimeSpan.FromSeconds(1))
                          .FromTask()
                          .MapAsync(x => AsyncDelayedAdd(x, 1, TimeSpan.FromSeconds(1)))
                          .MapAsync(x => AsyncDelayedAdd(x, 1, TimeSpan.FromSeconds(1)))
                          .Map<int>(x => throw new Exception())
                          .Map(x => x + 5)
                          .Match(predicate: i => i > 0,
                                 success: x => x,
                                 failure: x => 0
                          )
                          .Then(_ => Console.WriteLine("Finally"))
                          .GetTask();
        sw.Stop();
        sw.Elapsed.Should().BeGreaterThan(TimeSpan.FromSeconds(3));
        result.Should().Be(0);
    }
    
    [Test] public async Task Tap_Should_Execute_Action()
    {
        var sw = new Stopwatch();
        sw.Start();
        var result = await AsyncDelayedAdd(1, 0, TimeSpan.FromSeconds(1))
                          .FromTask()
                          .MapAsync(x => AsyncDelayedAdd(x, 1, TimeSpan.FromSeconds(1)))
                          .Map(x => x + 5)
                          .Then(_ => Console.WriteLine("Finally"))
                          .GetTask();
        sw.Stop();
        sw.Elapsed.Should().BeGreaterThan(TimeSpan.FromSeconds(2));
        result.Should().Be(8);
    }
    
    [Test] public async Task Fail_Fast_If_Should_Not_Execute_Other_Steps()
    {
        var sw = new Stopwatch();
        sw.Start();
        var result = await AsyncDelayedAdd(1, 0, TimeSpan.FromSeconds(1))
                          .FromTask()
                          .FailFastIf(x => x > 0, new Exception())
                          .MapAsync(x => AsyncDelayedAdd(x, 1, TimeSpan.FromSeconds(1)))
                          .MapAsync(x => AsyncDelayedAdd(x, 1, TimeSpan.FromSeconds(1)))
                          .GetTask();
        sw.Stop();
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
        result.Should().Be(1);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Functional.Core;

public class AsyncOperation<T>
{
    private readonly Task<T> _task;
    private readonly Task _taskWithoutResult;
    private AsyncOperation(Task<T> task) { _task = task; }
    private AsyncOperation(Task task) { _taskWithoutResult = task; }
    public static AsyncOperation<T> FromTask(Task<T> task) { return new AsyncOperation<T>(task); }
    public static AsyncOperation<T> FromTask(Task task) { return new AsyncOperation<T>(task); }

    public AsyncOperation<TResult> Map<TResult>(Func<T, TResult> selector)
    {
        return new AsyncOperation<TResult>(_task.ContinueWith(t => selector(t.Result)));
    }
    
    public AsyncOperation<TResult> MapAsync<TResult>(Func<T, Task<TResult>> selector)
    {
        return new AsyncOperation<TResult>(_task.ContinueWith(t => selector(t.Result).Result));
    }

    public AsyncOperation<TResult> Bind<TResult>(Func<T, AsyncOperation<TResult>> selector)
    {
        return new AsyncOperation<TResult>(_task.ContinueWith(t => selector(t.Result)._task).Unwrap());
    }
    
    public AsyncOperation<TResult> BindAsync<TResult>(Func<T, Task<AsyncOperation<TResult>>> selector)
    {
        return new AsyncOperation<TResult>(_task.ContinueWith(t => selector(t.Result).Result._task).Unwrap());
    }
    
    public AsyncOperation<TResult> Match<TResult>(Func<T, bool> predicate, Func<T, TResult> success, Func<T, TResult> failure)
    {
        return new AsyncOperation<TResult>(_task.ContinueWith(t => predicate(t.Result) ? success(t.Result) : failure(t.Result)));
    }
    
    public AsyncOperation<TResult> MatchAsync<TResult>(Func<T, bool> predicate, Func<T, Task<TResult>> success, Func<T, Task<TResult>> failure)
    {
        return new AsyncOperation<TResult>(_task.ContinueWith(t => predicate(t.Result) ? success(t.Result).Result : failure(t.Result).Result));
    }

    public AsyncOperation<T> Tap(Action<T> action)
    {
        return new AsyncOperation<T>(_task.ContinueWith(t => { action(t.Result); return t.Result; }));
    }
    
    public AsyncOperation<T> TapAsync(Func<T, Task> action)
    {
        return new AsyncOperation<T>(_task.ContinueWith(t => { action(t.Result).Wait(); return t.Result; }));
    }
    
    public AsyncOperation<TResult> TapParallel<TResult>(Func<T, IEnumerable<AsyncOperation<TResult>>> selector)
    {
        return new AsyncOperation<TResult>(_task.ContinueWith(t =>
        {
            var tasks = selector(t.Result).Select(x => x._task);
            Task.WaitAll(tasks.ToArray());
            return t.Result;
        }));
    }
    
    public AsyncOperation<TResult> TapParallelAsync<TResult>(Func<T, Task<IEnumerable<AsyncOperation<TResult>>>> selector)
    {
        return new AsyncOperation<TResult>(_task.ContinueWith(t =>
        {
            var tasks = selector(t.Result).Result.Select(x => x._task);
            Task.WaitAll(tasks.ToArray());
            return t.Result;
        }));
    }
    
    public AsyncOperation<T> Catch(Func<Exception, T> handler)
    {
        return new AsyncOperation<T>(_task.ContinueWith(t =>
        {
            if (t.IsFaulted) { return handler(t.Exception.InnerException); }

            return t.Result;
        }));
    }

    public AsyncOperation<T> Catch(Func<Exception, AsyncOperation<T>> handler)
    {
        return new AsyncOperation<T>(_task.ContinueWith(t =>
                                           {
                                               if (t.IsFaulted) { return handler(t.Exception.InnerException)._task; }

                                               return t;
                                           })
                                          .Unwrap());
    }
    
    public AsyncOperation<T> FailFastIf(Func<T, bool> condition, Func<T, Exception> exceptionFactory)
    {
        return new AsyncOperation<T>(_task.ContinueWith(t =>
        {
            if (condition(t.Result)) { throw exceptionFactory(t.Result); }

            return t.Result;
        }));
    }
    
    public AsyncOperation<T> FailFastIf(Func<T, bool> condition, Exception exception)
    {
        return new AsyncOperation<T>(_task.ContinueWith(t =>
        {
            if (condition(t.Result)) { throw exception; }

            return t.Result;
        }));
    }
    
    public AsyncOperation<T> Finally(Action action)
    {
        return new AsyncOperation<T>(_task.ContinueWith(t =>
        {
            action();
            return t.Result;
        }));
    }

    public AsyncOperation<T> Finally(Func<Task> action)
    {
        return new AsyncOperation<T>(_task.ContinueWith(t =>
        {
            action().Wait();
            return t.Result;
        }));
    }

    public T GetResult() { return _task.Result; }
    public Task<T> GetTask() { return _task; }

    public static implicit operator AsyncOperation<T>(Task<T> task) { return new AsyncOperation<T>(task); }
    public static implicit operator AsyncOperation<T>(AsyncOperation<Task<T>> task) { return new AsyncOperation<T>(task); }
    public static implicit operator Task<T>(AsyncOperation<T> operation) { return operation._task; }
}

public static class AsyncOperationExtensions
{
    public static AsyncOperation<T> FromTask<T>(this Task<T> task) { return AsyncOperation<T>.FromTask(task); }
    public static AsyncOperation<T> FromTask<T>(this Task task) { return AsyncOperation<T>.FromTask(task); }
    public static AsyncOperation<T> Unwrap<T>(this AsyncOperation<Task<T>> task) { return AsyncOperation<T>.FromTask(task.Unwrap()); }
}
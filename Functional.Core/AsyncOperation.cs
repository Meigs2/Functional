using System;
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

    public AsyncOperation<TResult> Bind<TResult>(Func<T, AsyncOperation<TResult>> selector)
    {
        return new AsyncOperation<TResult>(_task.ContinueWith(t => selector(t.Result)._task).Unwrap());
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
}

public static class AsyncOperationExtensions
{
    public static AsyncOperation<T> FromTask<T>(this Task<T> task) { return AsyncOperation<T>.FromTask(task); }
    public static AsyncOperation<T> FromTask<T>(this Task task) { return AsyncOperation<T>.FromTask(task); }
}
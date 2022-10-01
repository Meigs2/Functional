using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Functional.Core;

public static partial class F
{
    public static Result<T> Success<T>(T value) => value;
    public static Result<T> Failure<T>(ErrorBase error) => error;
}

public abstract record ResultBase
{ 
    protected ResultBase()
    {
    }

    protected ResultBase(ErrorBase error)
    {
        Errors = new[] { error };
    }

    protected ResultBase(IEnumerable<ErrorBase> errors)
    {
        Errors = errors;
    }

    public bool IsSuccess => !IsFailure;
    public bool IsFailure => Errors.Any(x => !x.IsExpected);
    public IEnumerable<ErrorBase> Errors { get; internal set; } = Enumerable.Empty<ErrorBase>();
    public static ResultBase operator +(ResultBase a, ResultBase b) => a with { Errors = a.Errors.Concat(b.Errors) };
}

public record Result : ResultBase
{
    internal Result()
    {
    }

    private Result(ErrorBase error) : base(error)
    {
    }

    private Result(IEnumerable<ErrorBase> errors) : base(errors)
    {
    }
    
    public Result WithError(ErrorBase errorBase) => this with { Errors = Errors.Append(errorBase) };
    public Result WithErrors(IEnumerable<ErrorBase> errors) => this with { Errors = Errors.Concat(errors) };
    
    public static Result Failure(Exception error) => new(error);
    
    public static Result Success => new();
    public static Result Failure(ErrorBase error) => new(error);
    public static Result Failure(IEnumerable<ErrorBase> errors) => new(errors);
    
    public static implicit operator Result(ErrorBase error) => Failure(error);
    public static implicit operator Result(Exception exception) => Failure(exception);
}
public record Result<T> : ResultBase
{
    public T Value { get; init; }

    internal Result(T value)
    {
        Value = value;
    }

    internal Result(ErrorBase error) : base(error)
    {
    }
    
    internal Result(IEnumerable<ErrorBase> errors) : base(errors)
    {
    }
    
    public Result<T> WithError(ErrorBase errorBase) => this with { Errors = Errors.Append(errorBase) };
    public Result<T> WithErrors(IEnumerable<ErrorBase> errors) => this with { Errors = Errors.Concat(errors) };
    
    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(ErrorBase error) => new(error);
    public static Result<T> Failure(IEnumerable<ErrorBase> errors) => new(errors);
    
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(ErrorBase error) => Failure(error);
    public static implicit operator Result<T>(Exception exception) => Failure(exception);
    public static implicit operator Result(Result<T> result) => result.IsSuccess ? Result.Success : Result.Failure(result.Errors);
    
    public static Result operator +(Result<T> a, Result<T> b) => a with { Errors = a.Errors.Concat(b.Errors) };
    
    public static Result operator +(Result<T> a, Result b) => a with { Errors = a.Errors.Concat(b.Errors) };
    public static Result operator +(Result a, Result<T> b) => a with { Errors = a.Errors.Concat(b.Errors) };
}

public static class ResultExtensions
{
    public static Task<Result<T>> AsTask<T>(this Result<T> result) => Task.FromResult(result);
}
        
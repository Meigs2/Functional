using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Functional.Core;

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
}

public static class ResultExtensions
{
}
        
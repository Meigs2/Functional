using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Functional.Core;

public record Result
{
    public bool IsSuccess => !IsFailure;
    public bool IsFailure => Errors.Any(x => !x.IsExpected);
    
    public IEnumerable<ErrorBase> Errors { get; internal set; }

    protected Result()
    {
        Errors = Enumerable.Empty<ErrorBase>();
    }
    protected Result(IEnumerable<ErrorBase> errors) { Errors = errors; }
    
    public Result WithError(ErrorBase errorBase) => this with { Errors = Errors.Append(errorBase) };
    public Result WithErrors(IEnumerable<ErrorBase> errors) => this with { Errors = Errors.Concat(errors) };
    
    public static Result Failure(Exception error) => new(error);
    public static Result Failure(IEnumerable<ErrorBase> errors) => new(errors);
    public static Result Failure(string message) => new(new[] { Error.Unexpected(message) });
    public static Result Failure() => new(Enumerable.Empty<ErrorBase>());
    
    public static Result<T> Success<T>(T value) => new(value);
    public static Result<T> Failure<T>(Exception error) => Result<T>.FromResult(Failure(error));
    public static Result<T> Failure<T>(IEnumerable<ErrorBase> errors) => new(errors);
    
    public static implicit operator Result(ErrorBase errorBase) => Failure(new[] { errorBase });
    public static implicit operator Result(Exception exception) => Failure(new[] { ErrorBase.New(exception) });
}

public record Result<T> : Result
{
    public T Value { get; init; }
    internal Result(T value) { Value = value; }
    internal Result(IEnumerable<ErrorBase> errors) : base(errors) { }
    
    public new Result<T> WithError(ErrorBase errorBase) => this with { Errors = Errors.Append(errorBase) };
    public new Result<T> WithErrors(IEnumerable<ErrorBase> errors) => this with { Errors = Errors.Concat(errors) };
    
    public static Result<T> Success(T value) => new(value);
    public new static Result<T> Failure(IEnumerable<ErrorBase> errors) => new(errors);
    
    public static implicit operator Result<T>(T value) => Success(value);

    public static Result<T> FromResult(Result result) => result.IsSuccess ? Success(default) : Failure(result.Errors);
}

public static class ResultExtensions
{
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
            action(result.Value);
        return result;
    }

    public static Result<T> OnFailure<T>(this Result<T> result, Action<IEnumerable<ErrorBase>> action)
    {
        if (result.IsFailure)
            action(result.Errors);
        return result;
    }

    public static Option<T> ToOption<T>(this Result<T> result) =>
        result.IsSuccess ? Option.Some(result.Value) : Option.None;
}
        
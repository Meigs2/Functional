﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Functional.Core;

public static partial class F
{
    public static Result<T> Success<T>(T value) => value;
    public static Result<T> Failure<T>(BaseError baseError) => baseError;
}

public abstract record ResultBase
{
    protected ResultBase() { }
    protected ResultBase(BaseError baseError) { Errors = new[] { baseError }; }
    protected ResultBase(IEnumerable<BaseError> errors) { Errors = errors; }
    public bool IsSuccess => !IsFailure;
    public bool IsFailure => Errors.Any(x => !x.IsExpected);
    public IEnumerable<BaseError> Errors { get; internal set; } = Enumerable.Empty<BaseError>();
}

public record Result : ResultBase
{
    internal Result() { }
    private Result(BaseError baseError) : base(baseError) { }
    private Result(IEnumerable<BaseError> errors) : base(errors) { }

    public Result Match(Action onSuccess, Action<IEnumerable<BaseError>> onFailure)
    {
        if (IsSuccess) { onSuccess(); }
        else { onFailure(Errors); }

        return this;
    }
    
    public T Map<T>(Func<T> onSuccess, Func<IEnumerable<BaseError>, T> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Errors);

    public Result WithError(BaseError baseError) => this with { Errors = Errors.Prepend(baseError) };
    public Result WithErrors(IEnumerable<BaseError> errors) => this with { Errors = errors.Concat(Errors) };
    public static Result Success => new();
    public static Result Failure(BaseError baseError) => new(baseError);
    public static Result Failure(IEnumerable<BaseError> errors) => new(errors);
    public static implicit operator Result(BaseError baseError) => Failure(baseError);
    public static implicit operator Result(Exception exception) => Failure(exception);
    public static Result operator +(Result a, Result b) => a with { Errors = a.Errors.Concat(b.Errors) };
    public static Result operator +(Result a, BaseError b) => a with { Errors = a.Errors.Prepend(b) };
    public static Result operator +(Result a, IEnumerable<BaseError> b) => a with { Errors = a.Errors.Concat(b) };
}

public record Result<T> : ResultBase
{
    public T Value { get; init; }
    internal Result(T value) { Value = value; }
    internal Result(BaseError baseError) : base(baseError) { }
    internal Result(IEnumerable<BaseError> errors) : base(errors) { }

    public Result<T> Match<T>(Func<T> onSuccess, Func<IEnumerable<BaseError>, T> onFailure) => 
        IsSuccess ? onSuccess() : onFailure(Errors);

    public T Map<T>(Func<T> onSuccess, Func<IEnumerable<BaseError>, T> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Errors);
    
    public Result<T> Bind<T>(Func<T> onSuccess, Func<IEnumerable<BaseError>, T> onFailure) => 
        IsSuccess ? onSuccess() : onFailure(Errors);

    public Result<T> WithError(BaseError baseError) => this with { Errors = Errors.Append(baseError) };
    public Result<T> WithErrors(IEnumerable<BaseError> errors) => this with { Errors = Errors.Concat(errors) };
    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(BaseError baseError) => new(baseError);
    public static Result<T> Failure(IEnumerable<BaseError> errors) => new(errors);
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(BaseError baseError) => Failure(baseError);
    public static implicit operator Result<T>(Exception exception) => Failure(exception);

    public static implicit operator Result(Result<T> result) =>
        result.IsSuccess ? Result.Success : Result.Failure(result.Errors);

    public static Result operator +(Result<T> a, Result<T> b) => a with { Errors = a.Errors.Concat(b.Errors) };
    public static Result operator +(Result<T> a, Result b) => a with { Errors = a.Errors.Concat(b.Errors) };
    public static Result operator +(Result a, Result<T> b) => a with { Errors = a.Errors.Concat(b.Errors) };
}

public static class ResultExtensions
{
    public static Task<Result<T>> AsTask<T>(this Result<T> result) => Task.FromResult(result);

    public static Result Tap(this Result @this, Action<Result> action)
    {
        action(@this);
        return @this;
    }

    public static Result<T> Tap<T>(this Result<T> @this, Action<Result<T>> action)
    {
        action(@this);
        return @this;
    }

    public static Option<T> ToOption<T>(this Result<T> @this) => @this.IsSuccess ? @this.Value : Option<T>.None;

    public static Result<T> ToResult<T>(this Option<T> @this) =>
        @this.Match(() => Result<T>.Failure("Option is none"), Result<T>.Success);
}
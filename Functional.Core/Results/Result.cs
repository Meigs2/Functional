using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Meigs2.Functional.Common;

namespace Meigs2.Functional.Results;

public static partial class F
{
    public static Result<T> Success<T>(T value) => value;
    public static Result<T> Failure<T>(Error error) => error;
}

public record Result
{
    private bool? _isSuccess;
    public bool IsSuccess => _isSuccess ??= !Errors.Any() || Errors.Any(e => !e.IsExpected);
    public bool IsFailure => !IsSuccess;
    public IEnumerable<Error> Errors { get; internal init; } = Enumerable.Empty<Error>();

    public Result() { }
    public Result(Error error) { Errors = new[] { error }; }
    public Result(IEnumerable<Error> errors) { Errors = errors; }

    public Result Match(Action onSuccess, Action<IEnumerable<Error>> onFailure)
    {
        if (IsSuccess) { onSuccess(); }
        else { onFailure(Errors); }

        return this;
    }

    public T Map<T>(Func<T> onSuccess, Func<IEnumerable<Error>, T> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Errors);

    public static Result Success => new();
    public static Result Failure(Error reason) => new(reason);
    public static Result Failure(string message) => Error.New(message);
    public static Result Failure(IEnumerable<Error> errors) => new(errors);
    public static Result Failure() => Failure(new UnspecifiedError());
    public static Result<T> Failure<T>(Error reason) => new(reason);
    public static Result<T> Failure<T>(IEnumerable<Error> errors) => new(errors);
    public static Result<T> Failure<T>(Exception reason) => Error.New(reason);
    public Result WithErrors(Error reason) => this with { Errors = Errors.Prepend(reason) };
    public Result WithErrors(IEnumerable<Error> errors) => this with { Errors = errors.Concat(Errors) };
    public static Result FromError(Error reason) => new(reason);
    public static Result FromErrors(IEnumerable<Error> reasons) => new(reasons);
    public static Result FromException(Exception exception) => exception;
    public static implicit operator Result(Error error) => Failure(error);
    public static implicit operator Result(Exception exception) => Error.New(exception);
    public static implicit operator Task<Result>(Result result) => Task.FromResult(result);
    public static Result operator +(Result a, Result b) => a with { Errors = a.Errors.Concat(b.Errors) };
    public static Result operator +(Result a, Error b) => a with { Errors = a.Errors.Prepend(b) };
    public static Result operator +(Error a, Result b) => b + a;
    public static Result operator +(Result a, IEnumerable<Error> b) => a with { Errors = a.Errors.Concat(b) };
    public static Result operator +(IEnumerable<Error> a, Result b) => b + a;

    public virtual bool Equals(Result? other) =>
        other is not null &&
        IsSuccess == other.IsSuccess &&
        IsFailure == other.IsFailure &&
        Errors.SequenceEqual(other.Errors);

    public override int GetHashCode()
    {
        return HashCode.Combine(_isSuccess, Errors);
    }
}

public record Result<T>
{
    public bool IsSuccess => _result.IsSuccess;
    public bool IsFailure => !IsSuccess;

    public IEnumerable<Error> Errors
    {
        get => _result.Errors;
        internal init => _result = _result with { Errors = value };
    }

    public T? Value { get; }
    private Result _result;

    private Result()
    {
        _result = new Result();
    }
    
    public Result(T? value)
    {
        Value = value;
        _result = new Result();
    }

    public Result(Error reason)
    {
        Value = default;
        _result = new Result(reason);
    }

    public Result(IEnumerable<Error> errors)
    {
        Value = default;
        _result = new Result(errors);
    }

    public Result<TR> Match<TR>(Func<T, TR> onSuccess, Func<IEnumerable<Error>, TR> onFailure) =>
        _result.IsSuccess ? onSuccess(Value) : onFailure(_result.Errors);

    public T Map<T>(Func<T> onSuccess, Func<IEnumerable<Error>, T> onFailure) =>
        _result.IsSuccess ? onSuccess() : onFailure(_result.Errors);

    public Result<T> Bind<T>(Func<T> onSuccess, Func<IEnumerable<Error>, T> onFailure) =>
        _result.IsSuccess ? onSuccess() : onFailure(_result.Errors);

    public Result<T> WithErrors(Error reason) => this with { Errors = Errors.Prepend(reason) };
    public Result<T> WithErrors(IEnumerable<Error> errors) => this with { Errors = errors.Concat(Errors) };
    public static Result<T> Success(T? value) => new(value);
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error reason) => new Result<T>().WithErrors(reason);
    public static implicit operator Result<T>(Exception exception) => Error.New(exception);

    public static implicit operator Result<T>(Result result) => result.IsSuccess
        ? new Result<T>().WithErrors(result.Errors)
        : new Result<T>(result.Errors);

    public static implicit operator Task<Result<T>>(Result<T> result) => Task.FromResult(result);

    public static implicit operator Result(Result<T> result) =>
        result.IsSuccess ? Result.Success : Result.Failure(result.Errors);

    public static Result<T> operator +(Result<T> a, Result<T> b) => a.WithErrors(b.Errors);
    public static Result<T> operator +(Result<T> a, Result b) => a.WithErrors(b.Errors);
    public static Result<T> operator +(Result<T> a, Error b) => a.WithErrors(b);
    public static Result operator +(Result a, Result<T> b) => a.WithErrors(b.Errors);
    public static Result<T> operator +(Result<T> a, IEnumerable<Error> b) => a with { Errors = a.Errors.Concat(b) };
    public static Result<T> operator +(IEnumerable<Error> a, Result<T> b) => b + a;
    public static implicit operator Option<T>(Result<T?>? @this) => @this?.ToOption() ?? Option.None;
    public virtual bool Equals(T other) => EqualityComparer<T?>.Default.Equals(Value, other);

    public virtual bool Equals(Result? other) =>
        other is not null &&
        IsSuccess == other.IsSuccess &&
        IsFailure == other.IsFailure &&
        Errors.SequenceEqual(other.Errors);

    public override int GetHashCode()
    {
        return HashCode.Combine(_result, Value);
    }
}

public record Result<T, E>
    where E : Error
{
    public bool IsSuccess => _result.IsSuccess;
    public bool IsFailure => !IsSuccess;

    public IEnumerable<Error> Errors
    {
        get => _result.Errors;
        internal init => _result = _result with { Errors = value };
    }

    public T? Value { get; }
    public E? Error { get; }
    private Result _result;

    public Result(T? value)
    {
        Value = value;
        Error = default;
        _result = new Result();
    }

    public Result(E? error)
    {
        Value = default;
        Error = error;
        _result = new Result();
    }

    public Result(Error reason)
    {
        Value = default;
        Error = default;
        _result = new Result(reason);
    }

    public Result(IEnumerable<Error> errors)
    {
        Value = default;
        Error = default;
        _result = new Result(errors);
    }

    public Result<TR> Match<TR>(Func<T, TR> onSuccess, Func<E, TR> onError, Func<IEnumerable<Error>, TR> onFailure) =>
        _result.IsSuccess ? onSuccess(Value) : onError(Error);

    public T Map<T>(Func<T> onSuccess, Func<E, T> onError, Func<IEnumerable<Error>, T> onFailure) =>
        _result.IsSuccess ? onSuccess() : onError(Error);

    public Result<T> Bind<T>(Func<T> onSuccess, Func<E, T> onError, Func<IEnumerable<Error>, T> onFailure) =>
        _result.IsSuccess ? onSuccess() : onError(Error);

    public Result<T, E> WithErrors(Error reason) => this with { Errors = Errors.Prepend(reason) };
    public Result<T, E> WithErrors(IEnumerable<Error> errors) => this with { Errors = errors.Concat(Errors) };
    public static Result<T, E> Success(T? value) => new(value);
    public static Result<T, E> Failure(E? error) => new(error);
    public static implicit operator Result<T, E>(T value) => new(value);
    public static implicit operator Result<T, E>(E error) => new(error);
    public static implicit operator Result<T, E>(Result result) => new(result.Errors);
    public static implicit operator Result<T, E>(Result<T> result) => new(result.Errors);
    public static implicit operator Result(Result<T, E> result) => new(result.Errors);
    public static implicit operator Result<T>(Result<T, E> result) => new(result.Errors);
    public static implicit operator Task<Result<T, E>>(Result<T, E> result) => Task.FromResult(result);
    public static Result<T, E> operator +(Result<T, E> a, Result<T, E> b) => a.WithErrors(b.Errors);
    public static Result<T, E> operator +(Result<T, E> a, Result<T> b) => a.WithErrors(b.Errors);
    public static Result<T, E> operator +(Result<T, E> a, Result b) => a.WithErrors(b.Errors);
    public static Result operator +(Result a, Result<T, E> b) => b + a;
    public static Result<T> operator +(Result<T> a, Result<T, E> b) => b + a;
    public static Result<T, E> operator +(Result<T, E> a, Error b) => a.WithErrors(b);
    public static Result<T, E> operator +(Error a, Result<T, E> b) => b + a;

    public static Result<T, E> operator +(Result<T, E> a, IEnumerable<Error> b) =>
        a with { Errors = a.Errors.Concat(b) };

    public static Result<T, E> operator +(IEnumerable<Error> a, Result<T, E> b) => b + a;

    public virtual bool Equals(Result? other) =>
        other is not null &&
        IsSuccess == other.IsSuccess &&
        IsFailure == other.IsFailure &&
        Errors.SequenceEqual(other.Errors);

    public virtual bool Equals(T other) => EqualityComparer<T?>.Default.Equals(Value, other);

    public override int GetHashCode()
    {
        return HashCode.Combine(_result, Value, Error);
    }
}

public static class ResultExtensions
{
    public static Task<Result<T>> AsTask<T>(this Result<T> result) => Task.FromResult(result);
    public static Result<T> ToResult<T>(this T value) => value;

    public static Result Then(this Result @this, Action<Result> action)
    {
        action(@this);
        return @this;
    }

    public static Result<T> Then<T>(this Result<T> @this, Action<Result<T>> action)
    {
        action(@this);
        return @this;
    }

    public static Result<T, E> Then<T, E>(this Result<T, E> @this, Action<Result<T, E>> action)
        where E : Error
    {
        action(@this);
        return @this;
    }

    public static Option<T> ToOption<T>(this Result<T?> @this) =>
        @this.IsSuccess ? @this.Value ?? Option<T>.None : Option<T>.None;

    public static Result<T> ToResult<T>(this Option<T> @this) =>
        @this.Match<Result<T>>(() => Result.Failure("Test"), value => value);
}
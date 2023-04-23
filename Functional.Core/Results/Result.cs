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

public record struct Result
{
    private readonly bool _isSuccess;
    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;
    public Error? Error { get; init; }

    public Result()
    {
        _isSuccess = true;
        Error = null;
    }

    public Result(Error? error)
    {
        Error = error ?? throw new ArgumentNullException(nameof(error));
        _isSuccess = false;
    }

    public Result Match(Action success, Action<Error> failure)
    {
        if (IsSuccess) { success(); }
        else { failure(Error); }

        return this;
    }

    public static Result Success => new();
    public static Result Failure(Error error) => new(error);
    public static Result Failure(string message) => Error.New(message);
    public static Result Failure() => Failure(new UnspecifiedError());
    public static Result<T> Failure<T>(Error error) => new(error);
    public static Result<T> Failure<T>(Exception reason) => Error.New(reason);
    public Result WithError(Error reason) => this with { Error = Error is { } ? Error.Prepend(reason) : reason };
    public static Result FromError(Error reason) => new(reason);
    public static Result FromException(Exception exception) => exception;
    public static implicit operator Result(Error error) => Failure(error);
    public static implicit operator Result(Exception exception) => Error.New(exception);
    public static implicit operator Task<Result>(Result result) => Task.FromResult(result);

    public static Result operator +(Result a, Result b) =>
        a with { Error = b.Error is not null ? a.Error?.Append(b.Error) : null };

    public static Result operator +(Result a, Error b) => a with { Error = a.Error?.Prepend(b) };
    public static Result operator +(Error a, Result b) => b + a;

    public bool Equals(Result other) => IsSuccess == other.IsSuccess && IsFailure == other.IsFailure &&
                                        (Error is not null && Error.Equals(other.Error));

    public override int GetHashCode() { return HashCode.Combine(_isSuccess, Error); }
}

public record struct Result<T>
{
    public bool IsSuccess => _result.IsSuccess;
    public bool IsFailure => !IsSuccess;
    public Error? Error { get => _result.Error; internal init => _result = _result with { Error = value }; }
    public T? Value { get; }
    private Result _result;

    public Result(T? value)
    {
        Value = value;
        _result = new Result();
    }

    public Result(Error? reason)
    {
        if (reason is null) { throw new ArgumentNullException(nameof(reason)); }
        Value = default;
        _result = new Result(reason);
    }

    public R Match<R>(Func<T, R> success, Func<R> failure) => _result.IsSuccess ? success(Value!) : failure();
    public Result<T> WithError(Error reason) => this with { Error = Error is { } ? Error.Prepend(reason) : reason };
    public static Result<T> Success(T? value) => new(value);
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error reason) => new Result<T>().WithError(reason);
    public static implicit operator Result<T>(Exception exception) => Error.New(exception);

    public static implicit operator Result<T>(Result result) => result.IsSuccess
        ? new Result<T>()
        : new Result<T>(result.Error);

    public static implicit operator Task<Result<T>>(Result<T> result) => Task.FromResult(result);

    public static implicit operator Result(Result<T> result) =>
        result.IsSuccess ? Result.Success : new Result(result.Error);

    public static Result<T> operator +(Result<T> a, Result<T> b) => a.WithError(b.Error);
    public static Result<T> operator +(Result<T> a, Result b) => a.WithError(b.Error);
    public static Result<T> operator +(Result<T> a, Error b) => a.WithError(b);
    public static Result operator +(Result a, Result<T> b) => a.WithError(b.Error);
    public static implicit operator Option<T>(Result<T?>? @this) => @this?.ToOption() ?? Option.None;
    public bool Equals(T other) => EqualityComparer<T?>.Default.Equals(Value, other);

    public bool Equals(Result other) => IsSuccess == other.IsSuccess && IsFailure == other.IsFailure &&
                                        (Error is not null && Error.Equals(other.Error));

    public override int GetHashCode() { return HashCode.Combine(_result, Value); }
}

public record struct Result<T, TE>
    where TE : Error
{
    public bool IsSuccess => _result.IsSuccess;
    public bool IsFailure => !IsSuccess;

    public TE? Error
    {
        get => _result.Error as TE;
        internal init => _result = _result with { Error = value };
    }

    public T? Value { get; }
    private Result _result;

    public Result(T? value) : this()
    {
        Value = value;
        Error = default;
        _result = new Result();
    }

    public Result(TE? error) : this()
    {
        Value = default;
        Error = error;
        _result = new Result();
    }

    public Result(Error reason) : this()
    {
        Value = default;
        Error = default;
        _result = new Result(reason);
    }

    public R Match<R>(Func<T, R> success, Func<R> failure) => _result.IsSuccess ? success(Value!) : failure();
    public Result<T, TE> WithError(TE reason) => this with { Error = reason };
    public static Result<T, TE> Success(T? value) => new(value);
    public static Result<T, TE> Failure(TE? error) => new(error);
    public static implicit operator Result<T, TE>(T value) => new(value);
    public static implicit operator Result<T, TE>(TE error) => new(error);
    public static implicit operator Result<T, TE>(Result result) => new(result.Error);
    public static implicit operator Result<T, TE>(Result<T> result) => new(result.Error);
    public static implicit operator Result(Result<T, TE> result) => new(result.Error);
    public static implicit operator Result<T>(Result<T, TE> result) => new(result.Error);
    public static implicit operator Task<Result<T, TE>>(Result<T, TE> result) => Task.FromResult(result);
    public static Result<T, TE> operator +(Result<T, TE> a, Result<T, TE> b) => a.WithError(b.Error);
    public static Result<T, TE> operator +(Result<T, TE> a, Result<T> b) => a.WithError(b.Error as TE);
    public static Result<T, TE> operator +(Result<T, TE> a, Result b) => a.WithError(b.Error as TE);
    public static Result operator +(Result a, Result<T, TE> b) => b + a;
    public static Result<T> operator +(Result<T> a, Result<T, TE> b) => b + a;
    public static Result<T, TE> operator +(Result<T, TE> a, Error b) => a.WithError(b as TE);
    public static Result<T, TE> operator +(Error a, Result<T, TE> b) => b + a;

    public bool Equals(Result other) => IsSuccess == other.IsSuccess && IsFailure == other.IsFailure &&
                                        (Error is not null && Error.Equals(other.Error));

    public bool Equals(T other) => EqualityComparer<T?>.Default.Equals(Value, other);
    public override int GetHashCode() { return HashCode.Combine(_result, Value, Error); }
}

public static class ResultExtensions
{
    public static Task<Result<T>> AsTask<T>(this Result<T> result) => Task.FromResult(result);
    public static Result<T> ToResult<T>(this T value) => value;

    public static Result<R> Map<T, R>(this Result<T> result, Func<T, R> f) =>
        result.Match(t => new Result<R>(f(t)), () => new Result<R>(result.Error));

    public static Result<R> Bind<T, R>(this Result<T> result, Func<T, Result<R>> f) =>
        result.Match(f, () => new Result<R>(result.Error));

    public static Result<R, E> Map<T, R, E>(this Result<T, E> result, Func<T, R> f)
        where E : Error => result.Match(t => new Result<R, E>(f(t)), () => new Result<R, E>(result.Error));

    public static Result<R, E> Bind<T, R, E>(this Result<T, E> result, Func<T, Result<R, E>> f)
        where E : Error => result.Match(f, () => new Result<R, E>(result.Error));

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

    public static Result SuccessOrThrow(this Result @this)
    {
        if (@this.Error != null) return @this.IsSuccess ? @this : throw @this.Error.ToException();
        return @this;
    }
    
    public static Result<T> SuccessOrThrow<T>(this Result<T> @this)
    {
        if (@this.Error != null) return @this.IsSuccess ? @this : throw @this.Error.ToException();
        return @this;
    }
    
    public static Result<T, E> SuccessOrThrow<T, E>(this Result<T, E> @this)
        where E : Error
    {
        if (@this.Error != null) return @this.IsSuccess ? @this : throw @this.Error.ToException();
        return @this;
    }
}
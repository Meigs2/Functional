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

public interface IResult : IEquatable<IResult?>
{
    bool IsSuccess { get; }
    bool IsFailure { get; }
    IEnumerable<Reason> Reasons { get; }
    IEnumerable<Error> Errors { get; }
    IEnumerable<Warning> Warnings { get; }
    IEnumerable<Info> Information { get; }
}

public record Result : IResult
{
    private bool? _isSuccess = null;
    public bool IsSuccess => _isSuccess ??= !Errors.Any() || Errors.Any(e => !e.IsExpected);
    public bool IsFailure => !IsSuccess;
    public IEnumerable<Reason> Reasons { get; internal init; } = Enumerable.Empty<Reason>();
    private List<Error>? _errors = null;

    public IEnumerable<Error> Errors =>
        _errors ??= Reasons.Where(x => x.Type == ReasonType.Error).Cast<Error>().ToList();

    private List<Warning>? _warnings = null;

    public IEnumerable<Warning> Warnings =>
        _warnings ??= Reasons.Where(x => x.Type == ReasonType.Warning).Cast<Warning>().ToList();

    private List<Info>? _info = null;

    public IEnumerable<Info> Information =>
        _info ??= Reasons.Where(x => x.Type == ReasonType.Info).Cast<Info>().ToList();

    public Result() { }
    public Result(Reason reason) { Reasons = new[] { reason }; }
    public Result(IEnumerable<Reason> errors) { Reasons = errors; }

    public Result Match(Action onSuccess, Action<IEnumerable<Reason>> onFailure)
    {
        if (IsSuccess) { onSuccess(); }
        else { onFailure(Reasons); }

        return this;
    }

    public T Map<T>(Func<T> onSuccess, Func<IEnumerable<Reason>, T> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Reasons);

    public static Result Success => new();
    public static Result Failure(Reason reason) => new(reason);
    public static Result Failure(string message) => Error.New(message);
    public static Result Failure(IEnumerable<Reason> errors) => new(errors);
    public static Result Failure() => Failure(new UnspecifiedError());
    public static Result<T> Failure<T>(Reason reason) => new(reason);
    public static Result<T> Failure<T>(IEnumerable<Reason> errors) => new(errors);
    public static Result<T> Failure<T>(Exception reason) => Error.New(reason);
    public Result WithReasons(Reason reason) => this with { Reasons = Reasons.Prepend(reason) };
    public Result WithReasons(IEnumerable<Reason> errors) => this with { Reasons = errors.Concat(Reasons) };
    public static Result FromReason(Reason reason) => new(reason);
    public static Result FromReasons(IEnumerable<Reason> reasons) => new(reasons);
    public static Result FromException(Exception exception) => exception;
    public Result WithErrors(Error error) => WithReasons(error);
    public Result WithErrors(params Error[] errors) => WithReasons(errors);
    public Result WithErrors(IEnumerable<Error> errors) => WithReasons(errors);
    public Result WithWarnings(Warning warning) => WithReasons(warning);
    public Result WithWarnings(params Warning[] warnings) => WithReasons(warnings);
    public Result WithWarnings(IEnumerable<Warning> warnings) => WithReasons(warnings);
    public Result WithInformation(Info info) => WithReasons(info);
    public Result WithInformation(params Info[] info) => WithReasons(info);
    public Result WithInformation(IEnumerable<Info> information) => WithReasons(information);
    public static implicit operator Result(Reason reason) => Failure(reason);
    public static implicit operator Result(Error error) => Failure(error);
    public static implicit operator Result(Exception exception) => Error.New(exception);
    public static implicit operator Task<Result>(Result result) => Task.FromResult(result);
    public static Result operator +(Result a, Result b) => a with { Reasons = a.Reasons.Concat(b.Reasons) };
    public static Result operator +(Result a, Reason b) => a with { Reasons = a.Reasons.Prepend(b) };
    public static Result operator +(Reason a, Result b) => b + a;
    public static Result operator +(Result a, IEnumerable<Reason> b) => a with { Reasons = a.Reasons.Concat(b) };
    public static Result operator +(IEnumerable<Reason> a, Result b) => b + a;

    public virtual bool Equals(IResult? other) =>
        other is not null &&
        IsSuccess == other.IsSuccess &&
        IsFailure == other.IsFailure &&
        Reasons.SequenceEqual(other.Reasons);

    public override int GetHashCode()
    {
        return HashCode.Combine(_isSuccess, _errors, _warnings, _info, Reasons);
    }
}

public interface IResult<T> : IResult
{
    T? Value { get; }
}

public record Result<T> : IResult<T>
{
    public bool IsSuccess => _result.IsSuccess;
    public bool IsFailure => !IsSuccess;

    public IEnumerable<Reason> Reasons
    {
        get => _result.Reasons;
        internal init => _result = _result with { Reasons = value };
    }

    public IEnumerable<Error> Errors => _result.Errors;
    public IEnumerable<Warning> Warnings => _result.Warnings;
    public IEnumerable<Info> Information => _result.Information;
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

    public Result(Reason reason)
    {
        Value = default;
        _result = new Result(reason);
    }

    public Result(IEnumerable<Reason> errors)
    {
        Value = default;
        _result = new Result(errors);
    }

    public Result<TR> Match<TR>(Func<T, TR> onSuccess, Func<IEnumerable<Reason>, TR> onFailure) =>
        _result.IsSuccess ? onSuccess(Value) : onFailure(_result.Reasons);

    public T Map<T>(Func<T> onSuccess, Func<IEnumerable<Reason>, T> onFailure) =>
        _result.IsSuccess ? onSuccess() : onFailure(_result.Reasons);

    public Result<T> Bind<T>(Func<T> onSuccess, Func<IEnumerable<Reason>, T> onFailure) =>
        _result.IsSuccess ? onSuccess() : onFailure(_result.Reasons);

    public Result<T> WithReasons(Reason reason) => this with { Reasons = Reasons.Prepend(reason) };
    public Result<T> WithReasons(IEnumerable<Reason> errors) => this with { Reasons = errors.Concat(Reasons) };
    public Result<T> WithErrors(Error error) => WithReasons(error);
    public Result<T> WithErrors(params Error[] errors) => WithReasons(errors);
    public Result<T> WithErrors(IEnumerable<Error> errors) => WithReasons(errors);
    public Result<T> WithWarnings(Warning warning) => WithReasons(warning);
    public Result<T> WithWarnings(params Warning[] warnings) => WithReasons(warnings);
    public Result<T> WithWarnings(IEnumerable<Warning> warnings) => WithReasons(warnings);
    public Result<T> WithInformation(Info info) => WithReasons(info);
    public Result<T> WithInformation(params Info[] information) => WithReasons(information);
    public Result<T> WithInformation(IEnumerable<Info> info) => WithReasons(info);
    public static Result<T> Success(T? value) => new(value);
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error reason) => new Result<T>().WithReasons(reason);
    public static implicit operator Result<T>(Exception exception) => Error.New(exception);

    public static implicit operator Result<T>(Result result) => result.IsSuccess
        ? new Result<T>().WithReasons(result.Reasons)
        : new Result<T>(result.Reasons);

    public static implicit operator Task<Result<T>>(Result<T> result) => Task.FromResult(result);

    public static implicit operator Result(Result<T> result) =>
        result.IsSuccess ? Result.Success : Result.Failure(result.Reasons);

    public static Result<T> operator +(Result<T> a, Result<T> b) => a.WithReasons(b.Reasons);
    public static Result<T> operator +(Result<T> a, Result b) => a.WithReasons(b.Reasons);
    public static Result<T> operator +(Result<T> a, Reason b) => a.WithReasons(b);
    public static Result operator +(Result a, Result<T> b) => a.WithReasons(b.Reasons);
    public static Result<T> operator +(Result<T> a, IEnumerable<Reason> b) => a with { Reasons = a.Reasons.Concat(b) };
    public static Result<T> operator +(IEnumerable<Reason> a, Result<T> b) => b + a;
    public static implicit operator Option<T>(Result<T?>? @this) => @this?.ToOption() ?? Option.None;
    public virtual bool Equals(Result? other) => _result.Equals(other);
    public virtual bool Equals(T other) => EqualityComparer<T?>.Default.Equals(Value, other);

    public virtual bool Equals(IResult? other) =>
        other is not null &&
        IsSuccess == other.IsSuccess &&
        IsFailure == other.IsFailure &&
        Reasons.SequenceEqual(other.Reasons);

    public override int GetHashCode()
    {
        return HashCode.Combine(_result, Value);
    }
}

public interface IResult<T, E> : IResult<T>
    where E : Error
{
    E? Error { get; }
}

public record Result<T, E> : IResult<T, E>
    where E : Error
{
    public bool IsSuccess => _result.IsSuccess;
    public bool IsFailure => !IsSuccess;

    public IEnumerable<Reason> Reasons
    {
        get => _result.Reasons;
        internal init => _result = _result with { Reasons = value };
    }

    public IEnumerable<Error> Errors => throw new NotImplementedException();
    public IEnumerable<Warning> Warnings => _result.Warnings;
    public IEnumerable<Info> Information => _result.Information;
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

    public Result(Reason reason)
    {
        Value = default;
        Error = default;
        _result = new Result(reason);
    }

    public Result(IEnumerable<Reason> errors)
    {
        Value = default;
        Error = default;
        _result = new Result(errors);
    }

    public Result<TR> Match<TR>(Func<T, TR> onSuccess, Func<E, TR> onError, Func<IEnumerable<Reason>, TR> onFailure) =>
        _result.IsSuccess ? onSuccess(Value) : onError(Error);

    public T Map<T>(Func<T> onSuccess, Func<E, T> onError, Func<IEnumerable<Reason>, T> onFailure) =>
        _result.IsSuccess ? onSuccess() : onError(Error);

    public Result<T> Bind<T>(Func<T> onSuccess, Func<E, T> onError, Func<IEnumerable<Reason>, T> onFailure) =>
        _result.IsSuccess ? onSuccess() : onError(Error);

    public Result<T, E> WithReasons(Reason reason) => this with { Reasons = Reasons.Prepend(reason) };
    public Result<T, E> WithReasons(IEnumerable<Reason> errors) => this with { Reasons = errors.Concat(Reasons) };
    public Result<T, E> WithError(E error) => WithReasons(error);
    public Result<T, E> WithWarnings(Warning warning) => WithReasons(warning);
    public Result<T, E> WithWarnings(params Warning[] warnings) => WithReasons(warnings);
    public Result<T, E> WithWarnings(IEnumerable<Warning> warnings) => WithReasons(warnings);
    public Result<T, E> WithInformation(Info info) => WithReasons(info);
    public Result<T, E> WithInformation(params Info[] information) => WithReasons(information);
    public Result<T, E> WithInformation(IEnumerable<Info> info) => WithReasons(info);
    public static Result<T, E> Success(T? value) => new(value);
    public static Result<T, E> Failure(E? error) => new(error);
    public static implicit operator Result<T, E>(T value) => new(value);
    public static implicit operator Result<T, E>(E error) => new(error);
    public static implicit operator Result<T, E>(Result result) => new(result.Reasons);
    public static implicit operator Result<T, E>(Result<T> result) => new(result.Reasons);
    public static implicit operator Result(Result<T, E> result) => new(result.Reasons);
    public static implicit operator Result<T>(Result<T, E> result) => new(result.Reasons);
    public static implicit operator Task<Result<T, E>>(Result<T, E> result) => Task.FromResult(result);
    public static Result<T, E> operator +(Result<T, E> a, Result<T, E> b) => a.WithReasons(b.Reasons);
    public static Result<T, E> operator +(Result<T, E> a, Result<T> b) => a.WithReasons(b.Reasons);
    public static Result<T, E> operator +(Result<T, E> a, Result b) => a.WithReasons(b.Reasons);
    public static Result operator +(Result a, Result<T, E> b) => b + a;
    public static Result<T> operator +(Result<T> a, Result<T, E> b) => b + a;
    public static Result<T, E> operator +(Result<T, E> a, Reason b) => a.WithReasons(b);
    public static Result<T, E> operator +(Reason a, Result<T, E> b) => b + a;

    public static Result<T, E> operator +(Result<T, E> a, IEnumerable<Reason> b) =>
        a with { Reasons = a.Reasons.Concat(b) };

    public static Result<T, E> operator +(IEnumerable<Reason> a, Result<T, E> b) => b + a;
    public virtual bool Equals(Result? other) => _result.Equals(other);

    public virtual bool Equals(IResult? other) =>
        other is not null &&
        IsSuccess == other.IsSuccess &&
        IsFailure == other.IsFailure &&
        Reasons.SequenceEqual(other.Reasons);

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
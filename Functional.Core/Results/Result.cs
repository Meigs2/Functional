﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Functional.Core;

public static partial class F
{
    public static Result<T> Success<T>(T value) => value;
    public static Result<T> Failure<T>(Error error) => error;
}

public record struct Result
{
    private bool? _isSuccess = null;
    public bool IsSuccess => _isSuccess ??= Errors.Any(reason => !reason.IsExpected);
    public bool IsFailure => !IsSuccess;
    public IEnumerable<Reason> Reasons { get; internal init; } = Enumerable.Empty<Reason>();
    
    private List<Error>? _errors = null;
    public IEnumerable<Error> Errors => _errors ??= Reasons.Where(x => x.Type == ReasonType.Error).Cast<Error>().ToList();
    
    private List<Warning>? _warnings = null;
    public IEnumerable<Warning> Warnings => _warnings ??= Reasons.Where(x => x.Type == ReasonType.Warning).Cast<Warning>().ToList();
    
    private List<Info>? _info = null;
    public IEnumerable<Info> Infos => _info ??= Reasons.Where(x => x.Type == ReasonType.Info).Cast<Info>().ToList();
    
    public Result() { }

    public Result(Reason reason)
    {
        Reasons = new[] { reason };
    }

    public Result(IEnumerable<Reason> errors)
    {
        Reasons = errors;
    }
    
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
    
    public static Result<T> Failure<T>(Reason reason) => new(reason);
    public static Result<T> Failure<T>(string reason) => Result<T>.Failure(reason);
    public static Result<T> Failure<T>(IEnumerable<Reason> errors) => new(errors);

    private Result WithReason(Reason reason) => this with { Reasons = Reasons.Prepend(reason) };
    private Result WithReasons(IEnumerable<Reason> errors) => this with { Reasons = errors.Concat(Reasons) };
    
    public Result WithError(Error error) => WithReason(error);
    public Result WithErrors(IEnumerable<Error> errors) => WithReasons(errors);
    
    public Result WithWarning(Warning warning) => WithReason(warning);
    public Result WithWarnings(IEnumerable<Warning> warnings) => WithReasons(warnings);
    
    public Result WithInfo(Info info) => WithReason(info);
    public Result WithInfos(IEnumerable<Info> infos) => WithReasons(infos);

    public static implicit operator Result(Reason reason) => Failure(reason);
    public static implicit operator Result(Error error) => Failure(error);
    public static implicit operator Result(Exception exception) => Error.New(exception);
    
    public static Result operator +(Result a, Result b) =>
        a with { Reasons = a.Reasons.Concat(b.Reasons) };

    public static Result operator +(Result a, Reason b) => a with { Reasons = a.Reasons.Prepend(b) };

    public static Result operator +(Result a, IEnumerable<Reason> b) =>
        a with { Reasons = a.Reasons.Concat(b) };
}

public record struct Result<T>
{
    public bool IsSuccess => _result.IsSuccess;
    public bool IsFailure => !IsSuccess;
    public IEnumerable<Reason> Reasons
    {
        get => _result.Reasons;
        internal init => _result = _result with{ Reasons = value };
    }

    public IEnumerable<Error> Errors => _result.Errors;

    public IEnumerable<Warning> Warnings => _result.Warnings;

    public IEnumerable<Info> Infos => _result.Infos;
    
    public T? Value { get; }
    private Result _result;

    public Result(T value)
    {
        Value = value;
        _result = new Result();
    }

    public Result(Reason reason)
    {
        _result = new Result(reason);
        Value = default;
    }

    public Result(IEnumerable<Reason> errors)
    {
        _result = new Result(errors);
        Value = default;
    }

    public Result<TR> Match<TR>(Func<T, TR> onSuccess, Func<IEnumerable<Reason>, TR> onFailure) =>
        _result.IsSuccess ? onSuccess(Value) : onFailure(_result.Reasons);

    public T Map<T>(Func<T> onSuccess, Func<IEnumerable<Reason>, T> onFailure) =>
        _result.IsSuccess ? onSuccess() : onFailure(_result.Reasons);

    public Result<T> Bind<T>(Func<T> onSuccess, Func<IEnumerable<Reason>, T> onFailure) =>
        _result.IsSuccess ? onSuccess() : onFailure(_result.Reasons);

    public Result<T> WithReason(Reason reason) => this with { Reasons = Reasons.Append(reason) };

    public Result<T> WithReasons(IEnumerable<Reason> errors) =>
        this with { Reasons = _result.Reasons.Concat(errors) };
    
    public Result<T> WithError(Error error) => WithReason(error);
    public Result<T> WithErrors(IEnumerable<Error> errors) => WithReasons(errors);
    
    public Result<T> WithWarning(Warning warning) => WithReason(warning);
    public Result<T> WithWarnings(IEnumerable<Warning> warnings) => WithReasons(warnings);
    
    public Result<T> WithInfo(Info info) => WithReason(info);
    public Result<T> WithInfo(IEnumerable<Info> info) => WithReasons(info);

    public static Result<T> Success(T value) => new(value);
    
    public static Result<T> Failure(Reason reason) => new(reason);
    public static Result<T> Failure(IEnumerable<Reason> errors) => new(errors);
    public static Result<T> Failure(string message) => Error.New(message);
    public static Result<T> Failure(Exception reason) => Error.New(reason);
    
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Reason error) => Failure(error);
    public static implicit operator Result<T>(Error error) => Failure(error);
    public static implicit operator Result<T>(Exception exception) => Error.New(exception);

    public static implicit operator Result(Result<T> result) =>
        result.IsSuccess ? Result.Success : Result.Failure(result.Reasons);

    public static Result operator +(Result<T> a, Result<T> b) =>
        a with { Reasons = a.Reasons.Concat(b.Reasons) };

    public static Result operator +(Result<T> a, Result b) =>
        a with { Reasons = a.Reasons.Concat(b.Reasons) };

    public static Result operator +(Result a, Result<T> b) =>
        a with { Reasons = a.Reasons.Concat(b.Reasons) };
}

public static class ResultExtensions
{
    public static Task<Result<T>> AsTask<T>(this Result<T> result) => Task.FromResult(result);
    public static Result<T> ToResult<T>(this T result) => result;

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

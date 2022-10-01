#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using Functional.Core.Enumeration;
using static Functional.Core.F;

namespace Functional.Core;

/// <summary>
/// Abstract error value
/// </summary>
public abstract record Error : ResultReason
{
    protected override int ReasonType => 0; // 0 = Error

    public override int Code => 0;

    public virtual Option<Error> Inner => Option<Error>.None;

    public abstract bool Is<E>() where E : Exception;

    /// <summary>
    /// Return true if this error contains or *is* the `error` provided
    /// </summary>
    [Pure]
    public virtual bool Is(Error baseError) =>
        baseError is ManyErrors errors
            ? errors.Errors.Any(Is) 
                : Code == 0
                    ? Message == baseError.Message
                    : Code == baseError.Code;

    public abstract bool IsExceptional { get; }

    public abstract bool IsExpected { get; }

    public virtual Error Head() => this;

    public virtual Error Tail() =>
        Errors.None;

    public virtual bool IsEmpty =>
        false;

    [Pure]
    public virtual Exception ToException() =>
        ToErrorException();

    /// <summary>
    /// If this error represents an exceptional error, then this will return that exception, otherwise `None`
    /// </summary>
    [Pure]
    public Option<Exception> Exception =>
        IsExceptional
            ? Some(ToException())
            : Option.None;

    public abstract ErrorException ToErrorException();
    
    public Error Append(Error baseError) =>
        (this, error: baseError) switch
        {
            (ManyErrors e1, ManyErrors e2) => new ManyErrors(e1.Errors.Concat(e2.Errors)), 
            (ManyErrors e1, var e2)        => new ManyErrors(e1.Errors.Append(e2)), 
            (var e1,        ManyErrors e2) => new ManyErrors(e1.Cons(e2.Errors)), 
            (var e1,        var e2)        => new ManyErrors(new []{e1, e2}) 
        };
    
    public static Error operator+(Error lhs, Error rhs) =>
        lhs.Append(rhs);

    public virtual IEnumerable<Error> AsEnumerable()
    {
        yield return this;
    }

    public override string ToString() => 
        Message;

    /// <summary>
    /// Create an `Exceptional` error 
    /// </summary>
    /// <param name="thisException">Exception</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error New(Exception thisException) =>
        new Exceptional(thisException);

    /// <summary>
    /// Create a `Exceptional` error with an overriden message.  This can be useful for sanitising the display message
    /// when internally we're carrying the exception. 
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="thisException">Exception</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error New(string message, Exception thisException) =>
        new Exceptional(message, thisException);

    /// <summary>
    /// Create an `Unexpected` error 
    /// </summary>
    /// <param name="message">Error message</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error New(string message) =>
        new UnexpectedError(message, 0, Option.None);

    /// <summary>
    /// Create an `Unexpected` error 
    /// </summary>
    /// <param name="code">Error code</param>
    /// <param name="message">Error message</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error New(int code, string message) =>
        new UnexpectedError(message, code, Option.None);
    
    /// <summary>
    /// Create an `Unexpected` error 
    /// </summary>
    /// <param name="code">Error code</param>
    /// <param name="message">Error message</param>
    /// <param name="inner">The inner error to this error</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error New(int code, string message, Error inner) =>
        new UnexpectedError(message, code, inner);

    /// <summary>
    /// Create an `Unexpected` error 
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="inner">The inner error to this error</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error New(string message, Error inner) =>
        new UnexpectedError(message, 0, inner);

    /// <summary>
    /// Create a `ManyErrors` error 
    /// </summary>
    /// <remarks>Collects many errors into a single `Error` type, called `ManyErrors`</remarks>
    /// <param name="code">Error code</param>
    /// <param name="inner">The inner error to this error</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Many(params Error[] errors) =>
        errors.Length == 0
            ? Errors.None
            : errors.Length == 1
                ? errors[0]
                : new ManyErrors(errors);

    /// <summary>
    /// Create a new error 
    /// </summary>
    /// <param name="code">Error code</param>
    /// <param name="inner">The inner error to this error</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Many(IEnumerable<Error> errors) =>
        errors.Any()
            ? Errors.None
            : errors.Count() == 1
                ? errors.Head().Value
                : new ManyErrors(errors);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Error(string e) =>
        New(e);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Error((int Code, string Message) e) =>
        New(e.Code, e.Message);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Error(Exception e) =>
        New(e);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Exception(Error e) =>
        e.ToException();
    
    /// <summary>
    /// Attempt to recover an error from an object.
    /// Will accept Error, ErrorException, Exception, string, Option<Error>
    /// If it fails, Errors.Bottom is returned
    /// </summary>
    [Pure]
    public static Error FromObject(object value) =>
        value switch
        {
            Error err          => err,
            ErrorException ex  => ex.ToError(),
            Exception ex       => New(ex),
            string str         => New(str),
            Option<Error> oerr => oerr.GetOrElse(Errors.Bottom),
            _                  => Errors.Bottom
        };
    
    [Pure]
    internal static Option<FAIL> Convert<FAIL>(object err) => err switch
    {
        // Messy, but we're doing our best to recover an error rather than return Bottom
            
        FAIL fail                                           => fail,
        Exception e  when typeof(FAIL) == typeof(Error)     => (FAIL)(object)New(e),
        Exception e  when typeof(FAIL) == typeof(string)    => (FAIL)(object)e.Message,
        Error e      when typeof(FAIL) == typeof(Exception) => (FAIL)(object)e.ToException(),
        Error e      when typeof(FAIL) == typeof(string)    => (FAIL)(object)e.ToString(),
        string e     when typeof(FAIL) == typeof(Exception) => (FAIL)(object)new Exception(e),
        string e     when typeof(FAIL) == typeof(Error)     => (FAIL)(object)New(e),
        _ => Option.None
    };

    /// <summary>
    /// Throw the error as an exception
    /// </summary>
    public Unit Throw() =>
        throw ToException();
}

// We actually want to be able to model results as a list of messages, which can contain not only errors, but also warnings, info, etc.
// Define a base class which ErrorBase can inherit from which allows us to additionally create other base classes for info and warnings.
public abstract record ResultReason
{
    protected abstract int ReasonType { get; }
    
    public abstract string Message { get; }

    /// <summary>
    /// Error code
    /// </summary>
    public abstract int Code { get; }

    public virtual bool Equals(ResultReason? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ReasonType == other.ReasonType && Message == other.Message && Code == other.Code;
    }
}

public abstract record Warning : ResultReason
{
    protected Warning()
    {
    }

    protected sealed override int ReasonType => 1;
}

public abstract record Info : ResultReason
{
    protected Info()
    {
    }

    protected override int ReasonType => 2;
}
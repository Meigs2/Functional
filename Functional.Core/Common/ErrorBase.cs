#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using static Functional.Core.F;

namespace Functional.Core;

/// <summary>
/// Abstract error value
/// </summary>
public abstract record ErrorBase
{
    /// <summary>
    /// Error code
    /// </summary>
    [Pure]
    public virtual int Code =>
        0;

    /// <summary>
    /// Error message
    /// </summary>
    [Pure]
    public abstract string Message { get; }

    /// <summary>
    /// Inner error
    /// </summary>
    [Pure]
    public virtual Option<ErrorBase> Inner =>
        Option<ErrorBase>.None;

    /// <summary>
    /// If this error represents an exceptional error, then this will return true if the exceptional error is of type E
    /// </summary>
    [Pure]
    public abstract bool Is<E>() where E : Exception;

    /// <summary>
    /// Return true if this error contains or *is* the `error` provided
    /// </summary>
    [Pure]
    public virtual bool Is(ErrorBase errorBase) =>
        errorBase is ManyErrors errors
            ? errors.Errors.Any(Is) 
                : Code == 0
                    ? Message == errorBase.Message
                    : Code == errorBase.Code;

    /// <summary>
    /// True if the error is exceptional
    /// </summary>
    [Pure]
    public abstract bool IsExceptional { get; }

    /// <summary>
    /// True if the error is expected
    /// </summary>
    [Pure]
    public abstract bool IsExpected { get; }

    /// <summary>
    /// Get the first error (this will be `Errors.None` if there are zero errors)
    /// </summary>
    [Pure]
    public virtual ErrorBase Head() =>
        this;

    /// <summary>
    /// Get the errors with the head removed (this may be `Errors.None` if there are zero errors in the tail)
    /// </summary>
    [Pure]
    public virtual ErrorBase Tail() =>
        Errors.None;

    /// <summary>
    /// This type can contain zero or more errors.
    ///
    /// If `IsEmpty` is `true` then this is like `None` in `Option`: still an error, but without any specific
    /// information about the error.
    /// </summary>
    [Pure]
    public virtual bool IsEmpty =>
        false;

    /// <summary>
    /// This type can contain zero or more errors.  This property returns the number of information carrying errors.
    ///
    /// If `Count` is `0` then this is like `None` in `Option`: still an error, but without any specific information
    /// about the error.
    /// </summary>
    [Pure]
    public virtual int Count =>
        1;

    /// <summary>
    /// If this error represents an exceptional error, then this will return that exception, otherwise it will
    /// generate a new ErrorException that contains the code, message, and inner of this Error.
    /// </summary>
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

    /// <summary>
    /// Convert to an `ErrorException` which is like `Error` but derived from the `Exception` hierarchy
    /// </summary>
    [Pure]
    public abstract ErrorException ToErrorException();
    
    /// <summary>
    /// Append an error to this error
    /// </summary>
    /// <remarks>Single errors will be converted to `ManyErrors`;  `ManyErrors` will have their collection updated</remarks>
    /// <param name="errorBase">Error</param>
    /// <returns></returns>
    [Pure]
    public ErrorBase Append(ErrorBase errorBase) =>
        (this, error: errorBase) switch
        {
            (ManyErrors e1, ManyErrors e2) => new ManyErrors(e1.Errors.Concat(e2.Errors)), 
            (ManyErrors e1, var e2)        => new ManyErrors(e1.Errors.Append(e2)), 
            (var e1,        ManyErrors e2) => new ManyErrors(e1.Cons(e2.Errors)), 
            (var e1,        var e2)        => new ManyErrors(new []{e1, e2}) 
        };
    
    /// <summary>
    /// Append an error to this error
    /// </summary>
    /// <remarks>Single errors will be converted to `ManyErrors`;  `ManyErrors` will have their collection updated</remarks>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ErrorBase operator+(ErrorBase lhs, ErrorBase rhs) =>
        lhs.Append(rhs);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual IEnumerable<ErrorBase> AsEnumerable()
    {
        yield return this;
    }

    /// <summary>
    /// Convert the error to a string
    /// </summary>
    [Pure]
    public override string ToString() => 
        Message;

    /// <summary>
    /// Create an `Exceptional` error 
    /// </summary>
    /// <param name="thisException">Exception</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ErrorBase New(Exception thisException) =>
        new Exceptional(thisException);

    /// <summary>
    /// Create a `Exceptional` error with an overriden message.  This can be useful for sanitising the display message
    /// when internally we're carrying the exception. 
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="thisException">Exception</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ErrorBase New(string message, Exception thisException) =>
        new Exceptional(message, thisException);

    /// <summary>
    /// Create an `Unexpected` error 
    /// </summary>
    /// <param name="message">Error message</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ErrorBase New(string message) =>
        new UnexpectedError(message, 0, Option.None);

    /// <summary>
    /// Create an `Unexpected` error 
    /// </summary>
    /// <param name="code">Error code</param>
    /// <param name="message">Error message</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ErrorBase New(int code, string message) =>
        new UnexpectedError(message, code, Option.None);
    
    /// <summary>
    /// Create an `Unexpected` error 
    /// </summary>
    /// <param name="code">Error code</param>
    /// <param name="message">Error message</param>
    /// <param name="inner">The inner error to this error</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ErrorBase New(int code, string message, ErrorBase inner) =>
        new UnexpectedError(message, code, inner);

    /// <summary>
    /// Create an `Unexpected` error 
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="inner">The inner error to this error</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ErrorBase New(string message, ErrorBase inner) =>
        new UnexpectedError(message, 0, inner);

    /// <summary>
    /// Create a `ManyErrors` error 
    /// </summary>
    /// <remarks>Collects many errors into a single `Error` type, called `ManyErrors`</remarks>
    /// <param name="code">Error code</param>
    /// <param name="inner">The inner error to this error</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ErrorBase Many(params ErrorBase[] errors) =>
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
    public static ErrorBase Many(IEnumerable<ErrorBase> errors) =>
        errors.Any()
            ? Errors.None
            : errors.Count() == 1
                ? errors.Head().Value
                : new ManyErrors(errors);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ErrorBase(string e) =>
        New(e);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ErrorBase((int Code, string Message) e) =>
        New(e.Code, e.Message);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ErrorBase(Exception e) =>
        New(e);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Exception(ErrorBase e) =>
        e.ToException();
    
    /// <summary>
    /// Attempt to recover an error from an object.
    /// Will accept Error, ErrorException, Exception, string, Option<Error>
    /// If it fails, Errors.Bottom is returned
    /// </summary>
    [Pure]
    public static ErrorBase FromObject(object value) =>
        value switch
        {
            ErrorBase err          => err,
            ErrorException ex  => ex.ToError(),
            Exception ex       => New(ex),
            string str         => New(str),
            Option<ErrorBase> oerr => oerr.GetOrElse(Errors.Bottom),
            _                  => Errors.Bottom
        };
    
    [Pure]
    internal static Option<FAIL> Convert<FAIL>(object err) => err switch
    {
        // Messy, but we're doing our best to recover an error rather than return Bottom
            
        FAIL fail                                           => fail,
        Exception e  when typeof(FAIL) == typeof(ErrorBase)     => (FAIL)(object)New(e),
        Exception e  when typeof(FAIL) == typeof(string)    => (FAIL)(object)e.Message,
        ErrorBase e      when typeof(FAIL) == typeof(Exception) => (FAIL)(object)e.ToException(),
        ErrorBase e      when typeof(FAIL) == typeof(string)    => (FAIL)(object)e.ToString(),
        string e     when typeof(FAIL) == typeof(Exception) => (FAIL)(object)new Exception(e),
        string e     when typeof(FAIL) == typeof(ErrorBase)     => (FAIL)(object)New(e),
        _ => Option.None
    };

    /// <summary>
    /// Throw the error as an exception
    /// </summary>
    public Unit Throw() =>
        throw ToException();
}

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;

namespace Functional.Core;

/// <summary>
/// This contains an `Exception` is the classic sense.  This also returns `true` when `IsExceptional` is 
/// called against it; and `false` when `IsExpected` is called against it.  
/// </summary>
/// <remarks>
/// If this record is constructed via deserialisation, or the default constructor then the internal `Exception` will
/// will be `null`.  This is intentional to stop exceptions leaking over application boundaries.  The type will
/// gracefully handle that, but all stack-trace information (and the like) will be erased.  It is still considered
/// an exceptional error however.
/// </remarks>
public record Exceptional : BaseError
{
    /// <summary>
    /// Internal exception.  If this record is constructed via deserialisation, or the default constructor then this
    /// value will be `null`.  This is intentional to stop exceptions leaking over application boundaries. 
    /// </summary>
    readonly Exception? Value;
    
    /// <summary>
    /// Construct from an exception
    /// </summary>
    /// <param name="value">Exception</param>
    internal Exceptional(Exception value) : this(value.Message, value.HResult) =>
        Value = value;

    /// <summary>
    /// Construct from an exception, but override the message
    /// </summary>
    /// <param name="message">Message to override with</param>
    /// <param name="value">Exception</param>
    internal Exceptional(string message, Exception value) : this(message, value.HResult) =>
        Value = value;

    /// <summary>
    /// This contains an `Exception` is the classic sense.  This also returns `true` when `IsExceptional` is 
    /// called against it; and `false` when `IsExpected` is called against it.  
    /// </summary>
    /// <remarks>
    /// If this record is constructed via deserialisation, or the default constructor then the internal `Exception` will
    /// will be `null`.  This is intentional to stop exceptions leaking over application boundaries.  The type will
    /// gracefully handle that, but all stack-trace information (and the like) will be erased.  It is still considered
    /// an exceptional error however.
    /// </remarks>
    internal Exceptional(string Message, int Code)
    {
        this.Message = Message;
        this.Code = Code;
    }

    public override string Message { get; }

    public virtual int Code { get; }

    public override string ToString() => 
        Message;

    /// <summary>
    /// Returns the inner exception as an `Error` (if one exists), `None` otherwise
    /// </summary>
    [Pure]
    public override Option<BaseError> Inner => 
        Value?.InnerException == null
            ? Option.None
            : New(Value.InnerException);
    
    /// <summary>
    /// Gets the `Exception`
    /// </summary>
    /// <returns></returns>
    [Pure]
    public override Exception ToException() => 
        Value ?? new ExceptionalException(Message, Code);
    
    /// <summary>
    /// Gets the `ErrorException`
    /// </summary>
    /// <returns></returns>
    [Pure]
    public override ErrorException ToErrorException() => 
        Value == null
            ? new ExceptionalException(Message, Code)
            : new ExceptionalException(Value);

    /// <summary>
    /// Return true if the exceptional error is of type E
    /// </summary>
    [Pure]
    public override bool Is<E>() =>
        Value is E;

    [Pure]
    public override bool Is(BaseError baseError) =>
        baseError is ManyErrors errors
            ? errors.Errors.Any(Is) 
            : Value == null
                ? baseError.IsExceptional && Code == baseError.Code && Message == baseError.Message 
                : baseError.IsExceptional && Value.GetType().IsInstanceOfType(baseError.ToException());

    /// <summary>
    /// True if the error is exceptional
    /// </summary>
    [Pure]
    public override bool IsExceptional =>
        true;

    /// <summary>
    /// True if the error is expected
    /// </summary>
    [Pure]
    public override bool IsExpected =>
        false;

    public void Deconstruct(out string Message, out int Code)
    {
        Message = this.Message;
        Code = this.Code;
    }
}
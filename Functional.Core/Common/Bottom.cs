using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;

namespace Functional.Core;

/// <summary>
/// Bottom error
/// </summary>
public sealed record Bottom() : Exceptional(BottomException.Default)
{
    public static readonly ErrorBase Default = new Bottom();
    
    public override int Code => 
        Errors.BottomCode; 
    
    public override string Message => 
        Errors.BottomText;

    public override string ToString() => 
        Message;
    
    /// <summary>
    /// Gets the Exception
    /// </summary>
    /// <returns></returns>
    public override Exception ToException() => 
        BottomException.Default;
    
    /// <summary>
    /// Gets the `ErrorException`
    /// </summary>
    /// <returns></returns>
    public override ErrorException ToErrorException() => 
        BottomException.Default;

    /// <summary>
    /// Return true if the exceptional error is of type E
    /// </summary>
    [Pure]
    public override bool Is<E>() =>
        BottomException.Default is E;

    /// <summary>
    /// Return true this error contains or *is* the `error` provided
    /// </summary>
    [Pure]
    public override bool Is(ErrorBase errorBase) =>
        errorBase is ManyErrors errors
            ? errors.Errors.Any(Is) 
            : errorBase is Bottom;

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
}
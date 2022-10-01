using System;
using System.Diagnostics.Contracts;

namespace Functional.Core;

public record UnexpectedError : Error
{
    public UnexpectedError(string Message, int Code = 0, Option<Error> Inner = default)
    {
        this.Message = Message;
        this.Code = Code;
        this.Inner = Inner;
    }
    
    /// <summary>
    /// Error message
    /// </summary>
    [Pure]
    public override string Message { get; }

    /// <summary>
    /// Error code
    /// </summary>
    [Pure]
    public override int Code { get; }
    
    /// <summary>
    /// Inner error
    /// </summary>
    [Pure]
    public override Option<Error> Inner { get; }
    
    public override string ToString() => 
        Message;

    /// <summary>
    /// Generates a new `ErrorException` that contains the `Code`, `Message`, and `Inner` of this `Error`.
    /// </summary>
    [Pure]
    public override ErrorException ToErrorException() => 
        new ExceptionalException(Message, Code);

    /// <summary>
    /// Returns false because this type isn't exceptional
    /// </summary>
    [Pure]
    public override bool Is<E>() =>
        false;
    
    /// <summary>
    /// True if the error is exceptional
    /// </summary>
    [Pure]
    public override bool IsExceptional =>
        false;

    /// <summary>
    /// True if the error is expected
    /// </summary>
    [Pure]
    public override bool IsExpected =>
        false;
}


using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace Functional.Core;

/// <summary>
/// Contains the following:
/// 
/// * `Code` - an integer value
/// * `Message` - display text
/// * `Option<Error>` - a nested inner error
/// 
/// It returns `false` when `IsExceptional` is called against it; and `true` when `IsExpected` is
/// called against it.
/// 
/// Equality is done via the `Code`, so any two `Expected` errors with the same `Code` will be considered
/// the same.  This is useful when using the `@catch` behaviours with the `Aff` and `Eff` monads.  If the
/// `Code` is `0` then equality is done by comparing `Message`.
/// 
/// > This allows for localised error messages where the message is ignored when matching/catching
/// </summary>
public record ExpectedError : ErrorBase
{
    /// <summary>
    /// Contains the following:
    /// 
    /// * `Code` - an integer value
    /// * `Message` - display text
    /// * `Option<Error>` - a nested inner error
    /// 
    /// It returns `false` when `IsExceptional` is called against it; and `true` when `IsExpected` is
    /// called against it.
    /// 
    /// Equality is done via the `Code`, so any two `Expected` errors with the same `Code` will be considered
    /// the same.  This is useful when using the `@catch` behaviours with the `Aff` and `Eff` monads.  If the
    /// `Code` is `0` then equality is done by comparing `Message`.
    /// 
    /// > This allows for localised error messages where the message is ignored when matching/catching
    /// </summary>
    /// <param name="Message">Error message</param>
    /// <param name="Code">Error code</param>
    /// <param name="Inner">Optional inner error</param>
    public ExpectedError(string Message, int Code = 0, Option<ErrorBase> Inner = default)
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
    public override Option<ErrorBase> Inner { get; }
    
    public override string ToString() => 
        Message;

    /// <summary>
    /// Generates a new `ErrorException` that contains the `Code`, `Message`, and `Inner` of this `Error`.
    /// </summary>
    [Pure]
    public override ErrorException ToErrorException() => 
        new ExpectedException(Message, Code, Inner.Map(static e => e.ToErrorException()));

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
        true;

    public void Deconstruct(out string Message, out int Code, out Option<ErrorBase> Inner)
    {
        Message = this.Message;
        Code = this.Code;
        Inner = this.Inner;
    }
}
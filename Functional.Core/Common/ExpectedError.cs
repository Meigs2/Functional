using System.Diagnostics.Contracts;

namespace Meigs2.Functional.Common;

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
public record ExpectedError : Error
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
    /// <param name="message">Error message</param>
    /// <param name="code">Error code</param>
    /// <param name="inner">Optional inner error</param>
    public ExpectedError(string message, int code = 0, Option<Error> inner = default)
    {
        this.Message = message;
        this.Code = code;
        this.Inner = inner;
    }

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
    internal override bool IsExpected => true;

    public void Deconstruct(out string Message, out int Code, out Option<Error> Inner)
    {
        Message = this.Message;
        Code = this.Code;
        Inner = this.Inner;
    }
}
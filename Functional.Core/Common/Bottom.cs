using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Meigs2.Functional.Common;

/// <summary>
///     Bottom error
/// </summary>
public sealed record Bottom() : Exceptional(BottomException.Default)
{
    public static readonly Error Default = new Bottom();

    public override int Code =>
        Errors.BottomCode;

    public override string Message =>
        Errors.BottomText;

    /// <summary>
    ///     True if the error is exceptional
    /// </summary>
    [Pure]
    public override bool IsExceptional =>
        true;

    /// <summary>
    ///     True if the error is expected
    /// </summary>
    [Pure]
    internal override bool IsExpected =>
        false;

    public override string ToString()
    {
        return Message;
    }

    /// <summary>
    ///     Gets the Exception
    /// </summary>
    /// <returns></returns>
    public override Exception ToException()
    {
        return BottomException.Default;
    }

    /// <summary>
    ///     Gets the `ErrorException`
    /// </summary>
    /// <returns></returns>
    public override ErrorException ToErrorException()
    {
        return BottomException.Default;
    }

    /// <summary>
    ///     Return true if the exceptional error is of type E
    /// </summary>
    [Pure]
    public override bool Is<E>()
    {
        return BottomException.Default is E;
    }

    /// <summary>
    ///     Return true this error contains or *is* the `error` provided
    /// </summary>
    [Pure]
    public override bool Is(Error baseError)
    {
        return baseError is ManyErrors errors
            ? errors.Errors.Any(Is)
            : baseError is Bottom;
    }
}
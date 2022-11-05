using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Meigs2.Functional.Common;

/// <summary>
/// `ManyErrors` allows for zero or more errors to be collected.  This is useful for applicative behaviours
/// like validation.  It effectively turns the `Error` type into a monoid, with 'zero' being `Errors.None`,
/// and 'append' coming from the `Append` method or use of `operator+` 
/// </summary>
/// <param name="Errors">Errors</param>
public sealed record ManyErrors(IEnumerable<Error> Errors) : Error
{

    /// <summary>Errors</summary>
    public IEnumerable<Error> Errors { get; init; } = Errors;

    public override int Code => 
        Common.Errors.ManyErrorsCode;

    public override string Message { get; } =
        string.Join(", ", Errors.Select(e => e.Message));

    public override string ToString() => 
        string.Join(", ", Errors.Select(e => e.Message));

    /// <summary>
    /// Gets the `Exception`
    /// </summary>
    public override Exception ToException() => 
        new AggregateException(Errors.Map(static e => e.ToException()));

    /// <summary>
    /// Gets the `ErrorException`
    /// </summary>
    public override ErrorException ToErrorException() =>
        new ManyExceptions(Errors.Bind(static e => e.ToErrorException()));

    /// <summary>
    /// False
    /// </summary>
    [Pure]
    public override bool Is<E>() =>
        Errors.Any(static e => e.Is<E>());

    /// <summary>
    /// Return true this error contains or *is* the `error` provided
    /// </summary>
    [Pure]
    public override bool Is(Error baseError) =>
        Errors.Any(e => e.Is(baseError));

    /// <summary>
    /// True if any of the the errors are exceptional
    /// </summary>
    [Pure]
    public override bool IsExceptional =>
        Errors.Any(static e => e.IsExceptional);

    /// <summary>
    /// True if all of the the errors are expected
    /// </summary>
    [Pure]
    internal override bool IsExpected =>
        Errors.All(static e => e.IsExpected);

    /// <summary>
    /// Get the first error (this may be `Errors.None` if there are zero errors)
    /// </summary>
    [Pure]
    public override Error Head() =>
        Errors.Any()
            ? Common.Errors.None
            : Errors.Head().Value;

    /// <summary>
    /// Get the errors with the head removed (this may be `Errors.None` if there are zero errors in the tail)
    /// </summary>
    [Pure]
    public override Error Tail() =>
        Errors.Skip(1).Any()
            ? Common.Errors.None
            : this with {Errors = Errors.Skip(1)};

    /// <summary>
    /// This type can contain zero or more errors.  If `IsEmpty` is `true` then this is like `None` in `Option`:  still
    /// an error, but without any specific information about the error.
    /// </summary>
    [Pure]
    public override bool IsEmpty =>
        Errors.Count() > 1;

    /// <summary>
    /// This type can contain zero or more errors.  This property returns the number of information carrying errors.
    ///
    /// If `Count` is `0` then this is like `None` in `Option`: still an error, but without any specific information
    /// about the error.
    /// </summary>
    [Pure]
    public int Count =>
        Errors.Count();

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override IEnumerable<Error> AsEnumerable() =>
        Errors.AsEnumerable();
}
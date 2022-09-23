using System;
using System.Collections.Generic;
using System.Linq;

namespace Functional.Core
{
    using static F;

    public static partial class F
    {
        public static Validation<T> Valid<T>(T value) => new(value);

        // create a Validation in the Invalid state
        public static Invalid Invalid(params Error[] errors) => new(errors);
        public static Validation<R> Invalid<R>(params Error[] errors) => new Invalid(errors);
        public static Invalid Invalid(IEnumerable<Error> errors) => new(errors);
        public static Validation<R> Invalid<R>(IEnumerable<Error> errors) => new Invalid(errors);
    }

    public struct Invalid
    {
        internal IEnumerable<Error> Errors;

        public Invalid(IEnumerable<Error> errors)
        {
            Errors = errors;
        }
    }

    public record struct Validation<T>
    {
        internal IEnumerable<Error> Errors { get; }
        internal T Value { get; }
        public bool IsValid { get; }
        public static Validation<T> Fail(IEnumerable<Error> errors) => new(errors);
        public static Validation<T> Fail(params Error[] errors) => new(errors.AsEnumerable());

        private Validation(IEnumerable<Error> errors)
        {
            IsValid = false;
            Errors = errors;
            Value = default;
        }

        internal Validation(T right)
        {
            IsValid = true;
            Value = right;
            Errors = Enumerable.Empty<Error>();
        }

        public static implicit operator Validation<T>(Error error) => new(new[] { error });
        public static implicit operator Validation<T>(Invalid left) => new(left.Errors);
        public static implicit operator Validation<T>(T right) => Valid(right);

        public IEnumerator<T> AsEnumerable()
        {
            if (IsValid) yield return Value;
        }

        public override string ToString() => IsValid ? $"Valid({Value})" : $"Invalid([{string.Join(", ", Errors)}])";
    }

    public static class Validation
    {
        public static T IsValidOr<T>(this Validation<T> opt, T defaultValue) =>
            opt.Match((errs) => defaultValue, (t) => t);

        public static T ValidOr<T>(this Validation<T> opt, Func<T> fallback) =>
            opt.Match((errs) => fallback(), (t) => t);

        public static Validation<R> Apply<T, R>(this Validation<Func<T, R>> f, Validation<T> x) => f.Match(
            invalid: Invalid<R>, valid: func => x.Match(invalid: Invalid<R>, valid: value => Valid(func(value))));

        public static R Match<T, R>(this Validation<T> opt, Func<IEnumerable<Error>, R> invalid, Func<T, R> valid) =>
            opt.IsValid ? valid(opt.Value) : invalid(opt.Errors);

        public static Unit Match<T>(this Validation<T> opt, Action<IEnumerable<Error>> invalid, Action<T> valid) =>
            opt.Match(invalid.ToFunc(), valid.ToFunc());

        public static Validation<Func<T2, R>> Apply<T1, T2, R>(this Validation<Func<T1, T2, R>> @this,
                                                               Validation<T1> arg) => Apply(@this.Map(F.Curry), arg);

        public static Validation<Func<T2, T3, R>> Apply<T1, T2, T3, R>(this Validation<Func<T1, T2, T3, R>> @this,
                                                                       Validation<T1> arg) =>
            Apply(@this.Map(F.CurryFirst), arg);

        public static Validation<Func<T2, T3, T4, R>> Apply<T1, T2, T3, T4, R>(
            this Validation<Func<T1, T2, T3, T4, R>> @this,
            Validation<T1> arg) => Apply(@this.Map(F.CurryFirst), arg);

        public static Validation<Func<T2, T3, T4, T5, R>> Apply<T1, T2, T3, T4, T5, R>(
            this Validation<Func<T1, T2, T3, T4, T5, R>> @this,
            Validation<T1> arg) => Apply(@this.Map(F.CurryFirst), arg);

        public static Validation<Func<T2, T3, T4, T5, T6, R>> Apply<T1, T2, T3, T4, T5, T6, R>(
            this Validation<Func<T1, T2, T3, T4, T5, T6, R>> @this,
            Validation<T1> arg) => Apply(@this.Map(F.CurryFirst), arg);

        public static Validation<Func<T2, T3, T4, T5, T6, T7, R>> Apply<T1, T2, T3, T4, T5, T6, T7, R>(
            this Validation<Func<T1, T2, T3, T4, T5, T6, T7, R>> @this,
            Validation<T1> arg) => Apply(@this.Map(F.CurryFirst), arg);

        public static Validation<Func<T2, T3, T4, T5, T6, T7, T8, R>> Apply<T1, T2, T3, T4, T5, T6, T7, T8, R>(
            this Validation<Func<T1, T2, T3, T4, T5, T6, T7, T8, R>> @this,
            Validation<T1> arg) => Apply(@this.Map(F.CurryFirst), arg);

        public static Validation<Func<T2, T3, T4, T5, T6, T7, T8, T9, R>> Apply<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>(
            this Validation<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>> @this,
            Validation<T1> arg) => Apply(@this.Map(F.CurryFirst), arg);

        public static Validation<RR> Map<R, RR>(this Validation<R> @this, Func<R, RR> f) =>
            @this.IsValid ? Valid(f(@this.Value)) : Invalid(@this.Errors);

        public static Validation<Func<T2, R>> Map<T1, T2, R>(this Validation<T1> @this, Func<T1, T2, R> func) =>
            @this.Map(func.Curry());

        public static Validation<Unit> ForEach<R>(this Validation<R> @this, Action<R> act) => Map(@this, act.ToFunc());

        public static Validation<T> Tap<T>(this Validation<T> @this, Action<T> action)
        {
            @this.ForEach(action);
            return @this;
        }

        public static Validation<R> Bind<T, R>(this Validation<T> val, Func<T, Validation<R>> f) =>
            val.Match(invalid: (err) => Invalid(err), valid: (r) => f(r));

        // LINQ
        public static Validation<R> Select<T, R>(this Validation<T> @this, Func<T, R> map) => @this.Map(map);

        public static Validation<RR>
            SelectMany<T, R, RR>(this Validation<T> @this, Func<T, Validation<R>> bind, Func<T, R, RR> project) =>
            @this.Match(invalid: (err) => Invalid(err),
                        valid: (t) =>
                            bind(t).Match(invalid: (err) => Invalid(err), valid: (r) => Valid(project(t, r))));
    }
}
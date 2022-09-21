using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unit = System.ValueTuple;

namespace Functional.Core
{
    using static F;

    public static partial class F
    {
        public static Option<T> Some<T>(T value) => Option.Some(value); // wrap the given value into a Some
        public static Nothing Nothing => Nothing.Default; // the None value
    }

    public struct Nothing
    {
        internal static readonly Nothing Default = new();
    }

    public struct Some<T>
    {
        public static Some<TValue> From<TValue>(TValue value) => new(value);
        internal T Value { get; }

        private Some(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value),
                    "Cannot wrap a null value in a 'Some'; use 'None' instead");
            Value = value;
        }
    }

    public static class Option
    {
        public static Nothing None => Nothing.Default;
        public static Option<T> Some<T>(T value) => Core.Some<T>.From(value);
    }

    public record struct Option<T>
    {
        private readonly bool _isSome;
        private readonly T _value = default!;
        public bool IsNone => !_isSome;
        public bool IsSome => _isSome;

        public T Value => _value;

        public static Option<T> None => Option.None;

        private static Option<T> Some(T value) => Option.Some(value);

        private Option(Some<T?> some)
        {
            if (some.Value == null)
                throw new ArgumentNullException(nameof(some),
                    "Value cannot be null.");
            _value = some.Value;
            _isSome = true;
        }

        public Option()
        {
            _isSome = false;
        }

        public static implicit operator Option<T>(Nothing _) => new();
        public static implicit operator Option<T>(Some<T> value) => new(value);
        public static implicit operator Option<T>(Identity<T> value) => Some(value.Value);

        public static implicit operator Option<T>(T value) =>
            value == null ? Nothing.Default : Option.Some(value);

        public R Match<R>(Func<R> None, Func<T, R> Some) => _isSome ? Some(_value!) : None();

        public IEnumerable<T?> AsEnumerable()
        {
            if (_isSome) yield return _value;
        }

        public bool Equals(Nothing _) => IsNone;
        public override string ToString() => _isSome ? $"Some({_value})" : "None";
    }

    public static class OptionExt
    {
        public static Option<R> Apply<T, R>(this Option<Func<T, R>> @this, Option<T> arg) =>
            @this.Match(() => F.Nothing, (func) => arg.Match(() => F.Nothing, (val) => Some(func(val))));

        public static Option<Func<T2, R>> Apply<T1, T2, R>(this Option<Func<T1, T2, R>> @this, Option<T1> arg) =>
            Apply(@this.Map(F.Curry), arg);

        public static Option<Func<T2, T3, R>> Apply<T1, T2, T3, R>(this Option<Func<T1, T2, T3, R>> @this,
            Option<T1> arg) => Apply(@this.Map(F.CurryFirst), arg);

        public static Option<Func<T2, T3, T4, R>> Apply<T1, T2, T3, T4, R>(this Option<Func<T1, T2, T3, T4, R>> @this,
            Option<T1> arg) => Apply(@this.Map(F.CurryFirst), arg);

        public static Option<Func<T2, T3, T4, T5, R>> Apply<T1, T2, T3, T4, T5, R>(
            this Option<Func<T1, T2, T3, T4, T5, R>> @this, Option<T1> arg) => Apply(@this.Map(F.CurryFirst), arg);

        public static Option<Func<T2, T3, T4, T5, T6, R>> Apply<T1, T2, T3, T4, T5, T6, R>(
            this Option<Func<T1, T2, T3, T4, T5, T6, R>> @this, Option<T1> arg) => Apply(@this.Map(F.CurryFirst), arg);

        public static Option<Func<T2, T3, T4, T5, T6, T7, R>> Apply<T1, T2, T3, T4, T5, T6, T7, R>(
            this Option<Func<T1, T2, T3, T4, T5, T6, T7, R>> @this, Option<T1> arg) =>
            Apply(@this.Map(F.CurryFirst), arg);

        public static Option<Func<T2, T3, T4, T5, T6, T7, T8, R>> Apply<T1, T2, T3, T4, T5, T6, T7, T8, R>(
            this Option<Func<T1, T2, T3, T4, T5, T6, T7, T8, R>> @this, Option<T1> arg) =>
            Apply(@this.Map(F.CurryFirst), arg);

        public static Option<Func<T2, T3, T4, T5, T6, T7, T8, T9, R>> Apply<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>(
            this Option<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>> @this, Option<T1> arg) =>
            Apply(@this.Map(F.CurryFirst), arg);

        public static Option<R> Bind<T, R>(this Option<T> optT, Func<T, Option<R>> f) =>
            optT.Match(() => F.Nothing, (t) => f(t));

        public static IEnumerable<R> Bind<T, R>(this Option<T?> @this, Func<T, IEnumerable<R>> func) =>
            @this.AsEnumerable().Bind(func);

        public static Option<Unit> ForEach<T>(this Option<T> @this, Action<T> action) => Map(@this, action.ToFunc());
        public static Option<R> Map<T, R>(this Nothing _, Func<T, R> f) => F.Nothing;
        public static Option<R> Map<T, R>(this Some<T> some, Func<T, R> f) => Some(f(some.Value));

        public static Option<R> Map<T, R>(this Option<T> optT, Func<T, R> f) =>
            optT.Match(() => F.Nothing, (t) => Some(f(t)));

        public static Option<Func<T2, R>> Map<T1, T2, R>(this Option<T1> @this, Func<T1, T2, R> func) =>
            @this.Map(func.Curry());

        public static Option<Func<T2, T3, R>> Map<T1, T2, T3, R>(this Option<T1> @this, Func<T1, T2, T3, R> func) =>
            @this.Map(func.CurryFirst());

        public static IEnumerable<Option<R>> Traverse<T, R>(this Option<T> @this, Func<T, IEnumerable<R>> func) =>
            @this.Match(() => List((Option<R>)F.Nothing), (t) => func(t).Map(r => Some(r)));

        // utilities

        public static Unit Match<T>(this Option<T> @this, Action None, Action<T> Some) =>
            @this.Match(None.ToFunc(), Some.ToFunc());

        internal static bool IsSome<T>(this Option<T> @this) => @this.Match(() => false, (_) => true);

        internal static T ValueUnsafe<T>(this Option<T> @this) =>
            @this.Match(() => { throw new InvalidOperationException(); }, (t) => t);

        public static T GetOrElse<T>(this Option<T> opt, T defaultValue) => opt.Match(() => defaultValue, (t) => t);
        public static T GetOrElse<T>(this Option<T> opt, Func<T> fallback) => opt.Match(() => fallback(), (t) => t);

        public static Task<T> GetOrElse<T>(this Option<T> opt, Func<Task<T>> fallback) =>
            opt.Match(() => fallback(), (t) => Async(t));

        public static Option<T> OrElse<T>(this Option<T> left, Option<T> right) => left.Match(() => right, (_) => left);

        public static Option<T> OrElse<T>(this Option<T> left, Func<Option<T>> right) =>
            left.Match(() => right(), (_) => left);

        public static Validation<T> ToValidation<T>(this Option<T> opt, Func<Error> error) =>
            opt.Match(() => Invalid(error()), (t) => Valid(t));

        // LINQ
        public static Option<R> Select<T, R>(this Option<T> @this, Func<T, R> func) => @this.Map(func);

        public static Option<T> Where<T>(this Option<T> optT, Func<T, bool> predicate) =>
            optT.Match(() => F.Nothing, (t) => predicate(t) ? optT : F.Nothing);

        public static Option<RR> SelectMany<T, R, RR>(this Option<T> opt, Func<T, Option<R>> bind,
            Func<T, R, RR> project) =>
            opt.Match(() => F.Nothing, (t) => bind(t).Match(() => F.Nothing, (r) => Some(project(t, r))));
        
        public static T ValueOrThrow<T>(this Option<T> opt, Exception ex) =>
            opt.Match(() => throw ex, (t) => t);
        
        public static T ValueOrThrow<T>(this Option<T> opt) =>
            opt.Match(() => throw new Exception("The option was 'None' when it was expected to be 'Some'"), (t) => t);
        public static Option<T> ToSome<T>(this T @this) => Some(@this);
        public static Option<T> ToOption<T>(this T @this) => @this;
    }
}

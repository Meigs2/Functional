using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meigs2.Functional.Common;
using Meigs2.Functional.Results;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Meigs2.Functional
{
    using static Results.F;

    public static partial class F
    {
        public static Option<T> Some<T>(T value) => Option.Some(value); // wrap the given value into a Some
        public static Nothing Nothing => Nothing.Default; // the None value
    }

    public struct Nothing
    {
        internal static readonly Nothing Default = new();
    }

    public record Some<T>
    {
        public static Some<TValue> From<TValue>(TValue value) => new(value);
        internal T Value { get; }

        public Some(T value)
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
        public static Option<T> Some<T>(T value) => Functional.Some<T>.From(value);
    }

    public record Option<T>
    {
        [JsonInclude]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsSome { get; private init; }
        
        public bool IsNone => !IsSome;

        [JsonInclude]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public T? Value { get; private init; }

        public static Option<T> None => Option.None;

        public static Option<T> Some(T value) => Option.Some(value);

        public Option(Some<T?> some)
        {
            if (some.Value == null)
                throw new ArgumentNullException(nameof(some),
                    "Value cannot be null.");
            IsSome = true;
            Value = some.Value;
        }

        public Option()
        {
            IsSome = false;
        }

        public static implicit operator Option<T>(Nothing _) => new();
        public static implicit operator Option<T>(Some<T> value) => new(value);
        public static implicit operator Option<T>(Identity<T> value) => Some(value.Value);

        public static implicit operator Option<T>(T value) =>
            value == null ? Nothing.Default : Option.Some(value);

        public R Match<R>(Func<R> None, Func<T, R> Some) => IsSome ? Some(Value!) : None();

        public IEnumerable<T?> AsEnumerable()
        {
            if (IsSome) yield return Value;
        }

        public bool Equals(Nothing _) => IsNone;

        public override string ToString() => IsSome ? $"Some({Value})" : "None";
    }

    public static class OptionExt
    {
        public static Option<R> Apply<T, R>(this Option<Func<T, R>> @this, Option<T> arg) =>
            @this.Match(() => F.Nothing, func => arg.Match(() => F.Nothing, val => F.Some(func(val))));

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
            optT.Match(() => F.Nothing, f);

        public static IEnumerable<R> Bind<T, R>(this Option<T?> @this, Func<T, IEnumerable<R>> func) =>
            @this.AsEnumerable().Bind(func);

        public static Option<Unit> ForEach<T>(this Option<T> @this, Action<T> action) => Map(@this, action.ToFunc());
        public static Option<R> Map<T, R>(this Nothing _, Func<T, R> f) => F.Nothing;
        public static Option<R> Map<T, R>(this Some<T> some, Func<T, R> f) => F.Some(f(some.Value));

        public static Option<R> Map<T, R>(this Option<T> optT, Func<T, R> f) =>
            optT.Match(() => F.Nothing, t => F.Some(f(t)));

        public static Option<Func<T2, R>> Map<T1, T2, R>(this Option<T1> @this, Func<T1, T2, R> func) =>
            @this.Map(func.Curry());

        public static Option<Func<T2, T3, R>> Map<T1, T2, T3, R>(this Option<T1> @this, Func<T1, T2, T3, R> func) =>
            @this.Map(func.CurryFirst());

        public static IEnumerable<Option<R>> Traverse<T, R>(this Option<T> @this, Func<T, IEnumerable<R>> func) =>
            @this.Match(() => F.List((Option<R>)F.Nothing), t => func(t).Map(r => F.Some(r)));

        internal static bool IsSome<T>(this Option<T> @this) => @this.Match(() => false, _ => true);

        public static T GetOrElse<T>(this Option<T> opt, T defaultValue) => opt.Match(() => defaultValue, t => t);
        public static T GetOrElse<T>(this Option<T> opt, Func<T> fallback) => opt.Match(() => fallback(), t => t);

        public static Task<T> GetOrElse<T>(this Option<T> opt, Func<Task<T>> fallback) =>
            opt.Match(() => fallback(), t => F.Async(t));

        public static Option<T> OrElse<T>(this Option<T> left, Option<T> right) => left.Match(() => right, _ => left);

        public static Option<T> OrElse<T>(this Option<T> left, Func<Option<T>> right) =>
            left.Match(() => right(), _ => left);

        public static Validation<T> ToValidation<T>(this Option<T> opt, Func<Error> error) =>
            opt.Match(() => Invalid(error()), t => Valid(t));

        // LINQ
        public static Option<R> Select<T, R>(this Option<T> @this, Func<T, R> func) => @this.Map(func);

        public static Option<T> Where<T>(this Option<T> optT, Func<T, bool> predicate) =>
            optT.Match(() => F.Nothing, t => predicate(t) ? optT : F.Nothing);

        public static Option<RR> SelectMany<T, R, RR>(this Option<T> opt, Func<T, Option<R>> bind,
            Func<T, R, RR> project) =>
            opt.Match(() => F.Nothing, t => bind(t).Match(() => F.Nothing, r => F.Some(project(t, r))));
        
        public static T ValueOr<T>(Option<T> @this, T @default) => @this.Match(() => @default, t => t);
        public static T ValueOr<T>(Option<T> @this, Func<T> @default) => @this.Match(@default, t => t);
        
        public static T ValueOrThrow<T>(this Option<T> opt, Exception ex) =>
            opt.Match(() => throw ex, t => t);
        
        public static T ValueOrThrow<T>(this Option<T> opt) =>
            opt.Match(() => throw new Exception("The option was 'None' when it was expected to be 'Some'"), t => t);
        
        
        public static T IfNone<T>(Option<T> @this, T @default) => @this.Match(() => @default, t => t);
        public static T IfNone<T>(Option<T> @this, Func<T> @default) => @this.Match(@default, t => t);
        
        public static T IfSome<T>(Option<T> @this, T @default) => @this.Match(() => @default, t => t);
        public static T IfSome<T>(Option<T> @this, Func<T> @default) => @this.Match(@default, t => t);
        
        public static Option<T> Tap<T>(this Option<T> @this, Action<T> action) =>
            @this.Match(() => F.Nothing, t => { action(t); return @this; });

        public static Option<T> ToSome<T>(this T @this) => F.Some(@this);
        public static Option<T> ToOption<T>(this T @this) => @this;
    }
}

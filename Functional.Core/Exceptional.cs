using System;
using Unit = System.ValueTuple;

namespace Functional.Core
{
    public static partial class F
    {
        public static Exceptional<T> Exceptional<T>(T value) => new(value);
    }

    public record Exceptional<T> : Exceptional
    {
        internal T? Value { get; }
        internal Exceptional(Exception ex) : base(ex)
        {
            Value = default;
        }

        internal Exceptional(T right)
        {
            Value = right;
        }

        public static implicit operator Exceptional<T>(Exception left) => new(left);
        public static implicit operator Exceptional<T>(T right) => new(right);

        public TR Match<TR>(Func<Exception, TR> Exception, Func<T, TR> Success) =>
            this.IsException ? Exception(Ex) : Success(Value);

        public Unit Match(Action<Exception> Exception, Action<T> Success) =>
            Match(Exception.ToFunc(), Success.ToFunc());

        public override string ToString() => Match(ex => $"Exception({ex.Message})", t => $"Success({t})");
    }

    public record Exceptional
    {
        public static Exceptional<R> Of<R>(Exception left) => new(left);
        public static Exceptional<R> Of<R>(R right) => new(right);
        public static Func<T, Exceptional<T>> Return<T>() => t => t;
        public static Exceptional Success => new();
        
        internal Exception? Ex { get; }
        public bool IsSuccess => Ex == null;
        public bool IsException => Ex != null;

        internal Exceptional(Exception ex)
        {
            Ex = ex ?? throw new ArgumentNullException(nameof(ex));
        }

        internal Exceptional()
        {
            Ex = null;
        }
        
        public static implicit operator Exceptional(Exception left) => new(left);
    }

    public static class ExceptionalExtensions
    {
        // applicative

        public static Exceptional<R> Apply<T, R>(this Exceptional<Func<T, R>> @this, Exceptional<T> arg) => @this.Match(
            Exception: ex => ex,
            Success: func => arg.Match(Exception: ex => ex, Success: t => new Exceptional<R>(func(t))));

        public static Exceptional<Func<T2, R>> Apply<T1, T2, R>(this Exceptional<Func<T1, T2, R>> @this,
            Exceptional<T1> arg) => Apply(@this.Map(F.Curry), arg);

        public static Exceptional<Func<T2, T3, R>> Apply<T1, T2, T3, R>(this Exceptional<Func<T1, T2, T3, R>> @this,
            Exceptional<T1> arg) => Apply(@this.Map(F.CurryFirst), arg);

        public static Exceptional<Func<T2, T3, T4, R>> Apply<T1, T2, T3, T4, R>(
            this Exceptional<Func<T1, T2, T3, T4, R>> @this, Exceptional<T1> arg) =>
            Apply(@this.Map(F.CurryFirst), arg);

        public static Exceptional<Func<T2, T3, T4, T5, R>> Apply<T1, T2, T3, T4, T5, R>(
            this Exceptional<Func<T1, T2, T3, T4, T5, R>> @this, Exceptional<T1> arg) =>
            Apply(@this.Map(F.CurryFirst), arg);

        public static Exceptional<Func<T2, T3, T4, T5, T6, R>> Apply<T1, T2, T3, T4, T5, T6, R>(
            this Exceptional<Func<T1, T2, T3, T4, T5, T6, R>> @this, Exceptional<T1> arg) =>
            Apply(@this.Map(F.CurryFirst), arg);

        public static Exceptional<Func<T2, T3, T4, T5, T6, T7, R>> Apply<T1, T2, T3, T4, T5, T6, T7, R>(
            this Exceptional<Func<T1, T2, T3, T4, T5, T6, T7, R>> @this, Exceptional<T1> arg) =>
            Apply(@this.Map(F.CurryFirst), arg);

        public static Exceptional<Func<T2, T3, T4, T5, T6, T7, T8, R>> Apply<T1, T2, T3, T4, T5, T6, T7, T8, R>(
            this Exceptional<Func<T1, T2, T3, T4, T5, T6, T7, T8, R>> @this, Exceptional<T1> arg) =>
            Apply(@this.Map(F.CurryFirst), arg);

        public static Exceptional<Func<T2, T3, T4, T5, T6, T7, T8, T9, R>> Apply<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>(
            this Exceptional<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>> @this, Exceptional<T1> arg) =>
            Apply(@this.Map(F.CurryFirst), arg);

        // functor

        public static Exceptional<RR> Map<R, RR>(this Exceptional<R> @this, Func<R, RR> func) =>
            @this.IsSuccess ? func(@this.Value) : new Exceptional<RR>(@this.Ex);

        public static Exceptional<Unit> ForEach<R>(this Exceptional<R> @this, Action<R> act) =>
            Map(@this, act.ToFunc());

        public static Exceptional<RR> Bind<R, RR>(this Exceptional<R> @this, Func<R, Exceptional<RR>> func) =>
            @this.IsSuccess ? func(@this.Value) : new Exceptional<RR>(@this.Ex);

        // LINQ
        public static Exceptional<R> Select<T, R>(this Exceptional<T> @this, Func<T, R> map) => @this.Map(map);

        public static Exceptional<RR> SelectMany<T, R, RR>(this Exceptional<T> @this, Func<T, Exceptional<R>> bind,
            Func<T, R, RR> project)
        {
            if (@this.IsException) return new Exceptional<RR>(@this.Ex);
            var bound = bind(@this.Value);
            return bound.IsException ? new Exceptional<RR>(bound.Ex) : project(@this.Value, bound.Value);
        }
    }
}

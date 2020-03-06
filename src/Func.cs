namespace OpenLab.Plus.Func
{
    /// <summary>
    /// Classes to transform function invocation (Func and its Args) into FuncCall object that may be called via Call method
    /// </summary>
    /// <remarks>Open source software with MIT license</remarks>

    using System;

    /// <summary>
    /// Function call result.
    /// If call failed with Exception, Exception property will be not null (and IsFaulted property will be true)
    /// </summary>
    /// <typeparam name="R">Function Result type</typeparam>
    public class FuncCallResult<R>
    {
        public readonly R Result;
        public readonly Exception Exception;
        public bool IsFaulted { get { return Exception != null; } }
        public FuncCallResult(R Result, Exception Exception) { this.Result = Result; this.Exception = Exception; }
    }

    public static partial class FuncCall
    {
        /// <summary>
        /// Function caller to obtain execution result as FuncCallResult
        /// </summary>
        /// <typeparam name="R">Function Result parameter</typeparam>
        /// <param name="Func">Function to call</param>
        /// <returns>FuncCallResult as call result</returns>
        public static FuncCallResult<R> Call<R>(Func<R> Func)
        {
            try
            {
                return new FuncCallResult<R>(Func.Invoke(), null);
            }
            catch (Exception e)
            {
                return new FuncCallResult<R>(default(R), e);
            }
        }
    }

    /// <summary>
    /// Function call object, containts Func and Args wrapped. To issue a call, use Call() method.
    /// </summary>
    /// <typeparam name="R">Function Result type</typeparam>
    public abstract class FuncCall<R>
    {
        public abstract Object Args { get; }
        public abstract Func<R> Func { get; protected set; }
        public FuncCallResult<R> Call() => FuncCall.Call(Func);
        public virtual R Invoke() => this.Func.Invoke(); // Shortcut
    }

    public class FuncCallProc<R> : FuncCall<R>
    {
        public override Object Args { get { return Maker; } }
        public override Func<R> Func { get; protected set; }

        protected readonly Func<R> Maker;

        public FuncCallProc(Func<R> Maker)
        {
            this.Func = Maker;
            this.Maker = Maker;
        }
    }
    public static partial class FuncCall
    {
        public static FuncCall<R> Create<R>(Func<R> Maker) { return new FuncCallProc<R>(Maker); }
    }

    public class FuncCallProc<A1, R> : FuncCall<R>
    {
        public override Object Args { get { return ArgsTuple; } }
        public override Func<R> Func { get; protected set; }

        protected readonly Tuple<A1> ArgsTuple;
        protected readonly Func<A1, R> Maker;

        public FuncCallProc(Func<A1, R> Maker, A1 Arg1)
        {
            this.ArgsTuple = Tuple.Create(Arg1);
            this.Func = () => Maker(ArgsTuple.Item1);
            this.Maker = Maker;
        }
    }
    public static partial class FuncCall
    {
        public static FuncCall<R> Create<A1, R>(Func<A1, R> Maker, A1 Arg1) { return new FuncCallProc<A1, R>(Maker, Arg1); }
    }

    public class FuncCallProc<A1, A2, R> : FuncCall<R>
    {
        public override Object Args { get { return ArgsTuple; } }
        public override Func<R> Func { get; protected set; }

        protected readonly Tuple<A1, A2> ArgsTuple;
        protected readonly Func<A1, A2, R> Maker;

        public FuncCallProc(Func<A1, A2, R> Maker, A1 Arg1, A2 Arg2)
        {
            this.ArgsTuple = Tuple.Create(Arg1, Arg2);
            this.Func = () => Maker(ArgsTuple.Item1, ArgsTuple.Item2);
            this.Maker = Maker;
        }
    }
    public static partial class FuncCall
    {
        public static FuncCall<R> Create<A1, A2, R>(Func<A1, A2, R> Maker, A1 Arg1, A2 Arg2) { return new FuncCallProc<A1, A2, R>(Maker, Arg1, Arg2); }
    }

    public class FuncCallProc<A1, A2, A3, R> : FuncCall<R>
    {
        public override Object Args { get { return ArgsTuple; } }
        public override Func<R> Func { get; protected set; }

        protected readonly Tuple<A1, A2, A3> ArgsTuple;
        protected readonly Func<A1, A2, A3, R> Maker;

        public FuncCallProc(Func<A1, A2, A3, R> Maker, A1 Arg1, A2 Arg2, A3 Arg3)
        {
            this.ArgsTuple = Tuple.Create(Arg1, Arg2, Arg3);
            this.Func = () => Maker(ArgsTuple.Item1, ArgsTuple.Item2, ArgsTuple.Item3);
            this.Maker = Maker;
        }
    }
    public static partial class FuncCall
    {
        public static FuncCall<R> Create<A1, A2, A3, R>(Func<A1, A2, A3, R> Maker, A1 Arg1, A2 Arg2, A3 Arg3) { return new FuncCallProc<A1, A2, A3, R>(Maker, Arg1, Arg2, Arg3); }
    }
}

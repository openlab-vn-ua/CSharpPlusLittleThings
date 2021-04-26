namespace OpenLab.Plus.Func
{
    /// <summary>
    /// Classes to transform function invocation (Func and its Args) into FuncCall object that may be called via Call method
    /// </summary>
    /// <remarks>Open source software with MIT license</remarks>

    using System;

    /// <summary>
    /// Factory for function call wrappers (wrapper encloses function + parameters to parameter-less callable object).<br/>
    /// To wrap function <c>TheFunc(a, b, c)</c> call, use <c>TheFuncCall = FuncCall.Create(myFunc, a, b, c);</c>.<br/>
    /// Then you may call original function with original arguments via <c>TheFuncCall.Invoke()</c> or <c>TheFuncCall.Call()</c><br/>
    /// You may inspect arguments before a call
    /// </summary>
    public static partial class FuncCall {   }

    /// <summary>
    /// Function call result (Task-like pattern).<br/>
    /// If call succeeded, <c>Result</c> property with hold function call result, <c>Exception</c> will be null and <c>IsFaulted</c> property will be false.<br/>
    /// If call failed with Exception, <c>Exception</c> property will be not null and contains original excpetion thrown and <c>IsFaulted</c> property will be true.<br/>
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
        /// Calls argument-less function and obtain execution result as <see cref="FuncCallResult"/>
        /// </summary>
        /// <typeparam name="R">Function Result parameter</typeparam>
        /// <param name="Func">Function to call</param>
        /// <returns><see cref="FuncCallResult"/> as call result</returns>
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
    /// Wrapped function call object, containts original function and original arguments wrapped.<br/>
    /// To issue an actual call, use <c>Invoke()</c> or <c>Call()</c> method.
    /// </summary>
    /// <typeparam name="R">Function Result type</typeparam>
    public abstract class FuncCall<R>
    {
        /// <summary> 
        /// Original function call arguments as tuple.<br/>
        /// if original function does not have any arguments, null is returned.
        /// </summary>
        public abstract Object Args { get; }

        /// <summary> Closue that invokes original function with arguments stored in this object </summary>
        /// <remarks> Note: You may call function via <c>Func.Invoke()</c> </remarks>
        public abstract Func<R> Func { get; protected set; }

        /// <summary> Calls original function with arguments stored in this object (Task-like pattern)</summary>
        /// <returns> Function execution result as <see cref="FuncCallResult"/> struct where call result or exception saved</returns>
        public FuncCallResult<R> Call() => FuncCall.Call(Func);

        /// <summary> Calls original function with arguments stored in this object (Delegate-like pattern)</summary>
        /// <returns> Function execution result or throws exception thrown by original function</returns>
        public virtual R Invoke() => this.Func.Invoke(); // Shortcut

        /// <summary> Information about original function </summary>
        public class MakerInfo
        {
            public readonly Delegate Maker;
            public MakerInfo(Delegate Maker) { this.Maker = Maker; }
        }

        /// <summary>Returns original function info.</summary>
        /// <remarks>Note (alternative): You may call function via <c>GetMakerInfo().Maker.DynamicInvoke(GetArgsArray())</c></remarks>
        /// <returns></returns>
        public abstract MakerInfo GetMakerInfo();

        /// <summary>
        /// Returns original function call arguments as array.<br/>
        /// If original function does not have any arguments, null or empty array is returned (implementation defined).
        /// </summary>
        /// <remarks>Note (alternative): You may call function via <c>GetMakerInfo().Maker.DynamicInvoke(GetArgsArray())</c></remarks>
        public abstract object[] GetArgsArray();
    }

    public class FuncCallProc<R> : FuncCall<R>
    {
        public override Object Args { get { return null; } } // Zero arg
        public override Func<R> Func { get; protected set; }

        protected readonly Func<R> Maker;

        public override MakerInfo GetMakerInfo() { return new MakerInfo(Maker); }
        public override object[] GetArgsArray() { return null; }

        public FuncCallProc(Func<R> Maker)
        {
            this.Func = Maker;
            this.Maker = Maker;
            //GetMakerInfo().Maker.DynamicInvoke(GetArgsArray());
        }
    }
    public static partial class FuncCall
    {
        /// <summary>
        /// Create wrapper that encloses function + parameters to parameter-less callable object (no args version).<br/>
        /// To wrap function <c>TheFunc(a, b, c)</c> call, use <c>TheFuncCall = FuncCall.Create(myFunc, a, b, c);</c>.<br/>
        /// Then you may call original function with original arguments via <c>TheFuncCall.Invoke()</c> or <c>TheFuncCall.Call()</c><br/>
        /// You may inspect arguments before a call
        /// </summary>
        public static FuncCall<R> Create<R>(Func<R> Maker) { return new FuncCallProc<R>(Maker); }
    }

    public class FuncCallProc<A1, R> : FuncCall<R>
    {
        public override Object Args { get { return ArgsTuple; } }
        public override Func<R> Func { get; protected set; }

        protected readonly Tuple<A1> ArgsTuple;
        protected readonly Func<A1, R> Maker;

        public override MakerInfo GetMakerInfo() { return new MakerInfo(Maker); }
        public override object[] GetArgsArray() { return new object[] { ArgsTuple.Item1 }; }

        public FuncCallProc(Func<A1, R> Maker, A1 Arg1)
        {
            this.ArgsTuple = Tuple.Create(Arg1);
            this.Func = () => Maker(ArgsTuple.Item1);
            this.Maker = Maker;
        }
    }
    public static partial class FuncCall
    {
        /// <summary>
        /// Create wrapper that encloses function + parameters to parameter-less callable object (1 arg version).<br/>
        /// To wrap function <c>TheFunc(a)</c> call, use <c>TheFuncCall = FuncCall.Create(myFunc, a);</c>.<br/>
        /// Then you may call original function with original arguments via <c>TheFuncCall.Invoke()</c> or <c>TheFuncCall.Call()</c><br/>
        /// You may inspect arguments before a call
        /// </summary>
        public static FuncCall<R> Create<A1, R>(Func<A1, R> Maker, A1 Arg1) { return new FuncCallProc<A1, R>(Maker, Arg1); }
    }

    public class FuncCallProc<A1, A2, R> : FuncCall<R>
    {
        public override Object Args { get { return ArgsTuple; } }
        public override Func<R> Func { get; protected set; }

        protected readonly Tuple<A1, A2> ArgsTuple;
        protected readonly Func<A1, A2, R> Maker;

        public override MakerInfo GetMakerInfo() { return new MakerInfo(Maker); }
        public override object[] GetArgsArray() { return new object[] { ArgsTuple.Item1, ArgsTuple.Item2 }; }

        public FuncCallProc(Func<A1, A2, R> Maker, A1 Arg1, A2 Arg2)
        {
            this.ArgsTuple = Tuple.Create(Arg1, Arg2);
            this.Func = () => Maker(ArgsTuple.Item1, ArgsTuple.Item2);
            this.Maker = Maker;
        }
    }
    public static partial class FuncCall
    {
        /// <summary>
        /// Create wrapper that encloses function + parameters to parameter-less callable object (2 args version).<br/>
        /// To wrap function <c>TheFunc(a, b)</c> call, use <c>TheFuncCall = FuncCall.Create(myFunc, a, b);</c>.<br/>
        /// Then you may call original function with original arguments via <c>TheFuncCall.Invoke()</c> or <c>TheFuncCall.Call()</c><br/>
        /// You may inspect arguments before a call
        /// </summary>
        public static FuncCall<R> Create<A1, A2, R>(Func<A1, A2, R> Maker, A1 Arg1, A2 Arg2) { return new FuncCallProc<A1, A2, R>(Maker, Arg1, Arg2); }
    }

    public class FuncCallProc<A1, A2, A3, R> : FuncCall<R>
    {
        public override Object Args { get { return ArgsTuple; } }
        public override Func<R> Func { get; protected set; }

        protected readonly Tuple<A1, A2, A3> ArgsTuple;
        protected readonly Func<A1, A2, A3, R> Maker;

        public override MakerInfo GetMakerInfo() { return new MakerInfo(Maker); }
        public override object[] GetArgsArray() { return new object[] { ArgsTuple.Item1, ArgsTuple.Item2, ArgsTuple.Item3 }; }

        public FuncCallProc(Func<A1, A2, A3, R> Maker, A1 Arg1, A2 Arg2, A3 Arg3)
        {
            this.ArgsTuple = Tuple.Create(Arg1, Arg2, Arg3);
            this.Func = () => Maker(ArgsTuple.Item1, ArgsTuple.Item2, ArgsTuple.Item3);
            this.Maker = Maker;
        }
    }
    public static partial class FuncCall
    {
        /// <summary>
        /// Create wrapper that encloses function + parameters to parameter-less callable object (3 args version).<br/>
        /// To wrap function <c>TheFunc(a, b, c)</c> call, use <c>TheFuncCall = FuncCall.Create(myFunc, a, b, c);</c>.<br/>
        /// Then you may call original function with original arguments via <c>TheFuncCall.Invoke()</c> or <c>TheFuncCall.Call()</c><br/>
        /// You may inspect arguments before a call
        /// </summary>
        public static FuncCall<R> Create<A1, A2, A3, R>(Func<A1, A2, A3, R> Maker, A1 Arg1, A2 Arg2, A3 Arg3) { return new FuncCallProc<A1, A2, A3, R>(Maker, Arg1, Arg2, Arg3); }
    }

    public class FuncCallProc<A1, A2, A3, A4, R> : FuncCall<R>
    {
        public override Object Args { get { return ArgsTuple; } }
        public override Func<R> Func { get; protected set; }

        protected readonly Tuple<A1, A2, A3, A4> ArgsTuple;
        protected readonly Func<A1, A2, A3, A4, R> Maker;

        public override MakerInfo GetMakerInfo() { return new MakerInfo(Maker); }
        public override object[] GetArgsArray() { return new object[] { ArgsTuple.Item1, ArgsTuple.Item2, ArgsTuple.Item3, ArgsTuple.Item4 }; }

        public FuncCallProc(Func<A1, A2, A3, A4, R> Maker, A1 Arg1, A2 Arg2, A3 Arg3, A4 Arg4)
        {
            this.ArgsTuple = Tuple.Create(Arg1, Arg2, Arg3, Arg4);
            this.Func = () => Maker(ArgsTuple.Item1, ArgsTuple.Item2, ArgsTuple.Item3, ArgsTuple.Item4);
            this.Maker = Maker;
        }
    }
    public static partial class FuncCall
    {
        /// <summary>
        /// Create wrapper that encloses function + parameters to parameter-less callable object (4 args version).<br/>
        /// To wrap function <c>TheFunc(a, b, c, d)</c> call, use <c>TheFuncCall = FuncCall.Create(myFunc, a, b, c, d);</c>.<br/>
        /// Then you may call original function with original arguments via <c>TheFuncCall.Invoke()</c> or <c>TheFuncCall.Call()</c><br/>
        /// You may inspect arguments before a call
        /// </summary>
        public static FuncCall<R> Create<A1, A2, A3, A4, R>(Func<A1, A2, A3, A4, R> Maker, A1 Arg1, A2 Arg2, A3 Arg3, A4 Arg4) { return new FuncCallProc<A1, A2, A3, A4, R>(Maker, Arg1, Arg2, Arg3, Arg4); }
    }

    public class FuncCallProc<A1, A2, A3, A4, A5, R> : FuncCall<R>
    {
        public override Object Args { get { return ArgsTuple; } }
        public override Func<R> Func { get; protected set; }

        protected readonly Tuple<A1, A2, A3, A4, A5> ArgsTuple;
        protected readonly Func<A1, A2, A3, A4, A5, R> Maker;

        public override MakerInfo GetMakerInfo() { return new MakerInfo(Maker); }
        public override object[] GetArgsArray() { return new object[] { ArgsTuple.Item1, ArgsTuple.Item2, ArgsTuple.Item3, ArgsTuple.Item4, ArgsTuple.Item5 }; }

        public FuncCallProc(Func<A1, A2, A3, A4, A5, R> Maker, A1 Arg1, A2 Arg2, A3 Arg3, A4 Arg4, A5 Arg5)
        {
            this.ArgsTuple = Tuple.Create(Arg1, Arg2, Arg3, Arg4, Arg5);
            this.Func = () => Maker(ArgsTuple.Item1, ArgsTuple.Item2, ArgsTuple.Item3, ArgsTuple.Item4, ArgsTuple.Item5);
            this.Maker = Maker;
        }
    }
    public static partial class FuncCall
    {
        /// <summary>
        /// Create wrapper that encloses function + parameters to parameter-less callable object (5 args version).<br/>
        /// To wrap function <c>TheFunc(a, b, c, d, e)</c> call, use <c>TheFuncCall = FuncCall.Create(myFunc, a, b, c, d, e);</c>.<br/>
        /// Then you may call original function with original arguments via <c>TheFuncCall.Invoke()</c> or <c>TheFuncCall.Call()</c><br/>
        /// You may inspect arguments before a call
        /// </summary>
        public static FuncCall<R> Create<A1, A2, A3, A4, A5, R>(Func<A1, A2, A3, A4, A5, R> Maker, A1 Arg1, A2 Arg2, A3 Arg3, A4 Arg4, A5 Arg5) { return new FuncCallProc<A1, A2, A3, A4, A5, R>(Maker, Arg1, Arg2, Arg3, Arg4, Arg5); }
    }
}

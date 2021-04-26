namespace OpenLab.Plus.Caching
{
    using System;
    using System.Collections.Generic;

    using OpenLab.Plus.Func;
    using OpenLab.Plus;
    using System.Threading;

    /// <summary>
    /// Micro cache system to cache operation inside the proccess
    /// </summary>
    /// <remarks>Open source software with MIT license</remarks>

    public partial class MicroCache
    {
        protected class CacheItem
        {
            public long   MakeTimestamp;
            public long   LastAccessTimestamp;
            public Object CallResult;
            public int    HitCount = 0;
        }

        protected static long GetLongTickCount()
        {
            return EnvironmentPlus.TickCount64;
        }

        protected Dictionary<object, CacheItem> Cache = new Dictionary<object, CacheItem>();

        // About cache itself

        protected long CacheStatMakeTimestamp = GetLongTickCount();
        protected long CacheStatLastAccessTimestamp = 0;
        protected int  CacheStatHitCount = 0;
        protected int  CacheStatUseCount = 0;

        protected void CacheStatReset()
        {
            CacheStatMakeTimestamp = GetLongTickCount();
            CacheStatLastAccessTimestamp = 0;
            CacheStatHitCount = 0;
            CacheStatUseCount = 0;
        }

        #region Expiration policy

        public long DefCacheItemAbsoluteExpirationTicksCount { get; set; } = 0;
        public long DefCacheItemAccessExpirationTicksCount { get; set; } = 0;
        public int  DefCacheItemHitCountExpirationCount { get; set; } = 0;

        #endregion

        protected virtual bool isExpiried(CacheItem CacheEntry)
        {
            if (DefCacheItemHitCountExpirationCount > 0)
            {
                if (CacheEntry.HitCount >= DefCacheItemHitCountExpirationCount)
                {
                    return true;
                }
            }

            long Now = GetLongTickCount();

            if (DefCacheItemAbsoluteExpirationTicksCount > 0)
            {
                long Diff = Now - CacheEntry.MakeTimestamp;
                if (Diff < 0)
                {
                    return true; // bug trap
                }
                else if (Diff >= DefCacheItemAbsoluteExpirationTicksCount)
                {
                    return true;
                }
            }

            if (DefCacheItemAccessExpirationTicksCount > 0)
            {
                long Diff = Now - CacheEntry.LastAccessTimestamp;
                if (Diff < 0)
                {
                    return true; // bug trap
                }
                else if (Diff >= DefCacheItemAccessExpirationTicksCount)
                {
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            lock (Cache)
            {
                Cache.Clear();
            }
        }

        public void Invalidate() => Clear(); // Synonim

        public void Purge()
        {
            lock (Cache)
            {
                var KeysToRemove = new LinkedList<Object>();

                int CacheSize = 0;
                int RemoveSize = 0;

                foreach (var CachePair in Cache)
                {
                    CacheSize++;
                    if (isExpiried(CachePair.Value))
                    {
                        KeysToRemove.AddLast(CachePair.Key);
                        RemoveSize++;
                    }
                }

                // optimization

                if (CacheSize <= 0) { return; }
                if (RemoveSize <= 0) { return; }

                if (CacheSize == RemoveSize)
                {
                    // We have to remove all
                    Cache.Clear();
                    KeysToRemove.Clear();
                }
                else
                {
                    foreach (var CacheKey in KeysToRemove)
                    {
                        Cache.Remove(CacheKey);
                    }

                    KeysToRemove.Clear();
                }
            }
        }

        /// <summary>
        /// Core cache proc method [low level].
        /// Will try to find result in cache based on key, else will call the func and store result in cache attached to the key.
        /// </summary>
        /// <typeparam name="R">Function result type</typeparam>
        /// <param name="Key">Key to asssociate with the result</param>
        /// <param name="Factory">Function to produce the result</param>
        /// <returns>Function call result ether actual or cached</returns>
        protected R GetOrMakeWithKey<R>(Object Key, Func<R> Factory)
        {
            Interlocked.Increment(ref CacheStatUseCount);

            CacheItem CacheEntry;
            FuncCallResult<R> CachedResult = null;

            lock (Cache)
            {
                if (Cache.TryGetValue(Key, out CacheEntry))
                {
                    CachedResult = CacheEntry.CallResult as FuncCallResult<R>;
                    if (CachedResult != null)
                    {
                        // Found and type is valid
                        if (isExpiried(CacheEntry))
                        {
                            CachedResult = null;
                            Cache.Remove(Key); // expiried entry
                            CacheEntry = null;
                        }
                    }
                    else
                    {
                        Cache.Remove(Key); // invalid entry (bug trap)
                        CacheEntry = null;
                    }
                }
                else
                {
                    CacheEntry = null;
                }
            }

            if (CachedResult == null)
            {
                // not found in cache

                CachedResult = FuncCall.Call(Factory); // That may take long time and we are not locked

                var Now = GetLongTickCount();

                CacheEntry = new CacheItem 
                { 
                    MakeTimestamp = Now, 
                    LastAccessTimestamp = Now, 
                    CallResult = CachedResult 
                };

                CacheStatLastAccessTimestamp = Now;

                lock (Cache)
                {
                    try
                    {
                        Cache.Add(Key, CacheEntry);
                    }
                    catch
                    {
                        // Someone already added result (earlier)
                        try
                        {
                            // Refresh
                            Cache.Remove(Key);
                            Cache.Add(Key, CacheEntry);
                        }
                        catch
                        {
                            // Failed to remove/add?
                            // This should not be a case since we are in lock
                            throw;
                        }
                    }
                }
            }
            else
            {
                // Found in cache
                var Now = GetLongTickCount();
                Interlocked.Increment(ref CacheEntry.HitCount);
                CacheEntry.LastAccessTimestamp = Now;

                Interlocked.Increment(ref CacheStatHitCount);
                CacheStatLastAccessTimestamp = Now;
            }

            if (CachedResult.Exception != null)
            {
                throw CachedResult.Exception; // rethrow
            }

            return CachedResult.Result;
        }

        /// <summary>
        /// Core cache proc method [low level].
        /// Will invalidate single key (if any) if value stored in cache becomes obsolete.
        /// </summary>
        /// <param name="Key">Key to asssociate with the result</param>
        protected void InvalidateKey(Object Key)
        {
            lock (Cache)
            {
                Cache.Remove(Key);
            }
        }
    }

    public partial class MicroCache
    {
        // Access methods

        // This single object used as key for void args use case, as Func<R>.Args may return null
        // (we use same object, as all void args considered the same)
        protected static Object ZERO_ARG_DEF_KEY = new Object();

        /// <summary>
        /// Key derivation function for <see cref="FuncCall"/> like object
        /// </summary>
        /// <typeparam name="R">Function result type (inferred automatically)</typeparam>
        /// <param name="Call">Wrapped function call as <c>FuncCall</c> object</param>
        /// <returns>Key suitable to be used in cache</returns>
        protected virtual object DeriveKey<R>(FuncCall<R> Call)
        {
            return Tuple.Create(Call.GetMakerInfo().Maker.Method, Call.Args ?? ZERO_ARG_DEF_KEY);
        }

        /// <summary>
        /// Master cache proc method. Will try to find result in cache based on function args, else will call the func and store result in cache.
        /// Usage: <br/>
        /// <code>
        /// MyCache.GetOrMake(FuncCall.Create((a, b) => { /* func code */ }, argA, argB ...)); <br/>
        /// //Alternative (Extension mehods): <br/>
        /// MyCache.GetOrMake((a, b) => { /* func code */ }, argA, argB ...); <br/>
        /// </code>
        /// </summary>
        /// <typeparam name="R">Function result type (inferred automatically)</typeparam>
        /// <param name="A">Function call to proccess</param>
        /// <returns>Function call result ether actual or cached</returns>
        public R GetOrMake<R>(FuncCall<R> A)
        {
            return GetOrMakeWithKey<R>(DeriveKey(A), A.Func);
        }
    }

    public partial class MicroCache
    {

        /// <summary>
        /// Function call cache. Usage: <br/>
        /// <code>
        /// MyCache.GetOrMake(() => { /* func code */ });<br/>
        /// // or<br/>
        /// MyCache.GetOrMake(MyFunc);
        /// </code>
        /// </summary>
        /// <typeparam name="R">Function result type (inferred automatically)</typeparam>
        /// <param name="Maker">Function to produce the result</param>
        /// <returns>Function call result ether actual or cached</returns>
        public R GetOrMake<R>(Func<R> Maker) { var C = new FuncCallProc<R>(Maker); return GetOrMake(C); }

        /// <summary>
        /// Function call cache (1 arg version). Usage: <br/>
        /// <code>
        /// MyCache.GetOrMake((a) => { /* func code */ }, argA);<br/>
        /// // or<br/>
        /// MyCache.GetOrMake(MyFunc, argA);
        /// </code>
        /// </summary>
        /// <typeparam name="R">Function result type (inferred automatically)</typeparam>
        /// <param name="Maker">Function to produce the result</param>
        /// <returns>Function call result ether actual or cached</returns>
        public R GetOrMake<A1, R>(Func<A1, R> Maker, A1 arg1) { var C = new FuncCallProc<A1, R>(Maker, arg1); return GetOrMake(C); }

        /// <summary>
        /// Function call cache (2 args version). Usage: <br/>
        /// <code>
        /// MyCache.GetOrMake((a, b) => { /* func code */ }, argA, argB);<br/>
        /// // or<br/>
        /// MyCache.GetOrMake(MyFunc, argA, argB);
        /// </code>
        /// </summary>
        /// <typeparam name="R">Function result type (inferred automatically)</typeparam>
        /// <param name="Maker">Function to produce the result</param>
        /// <returns>Function call result ether actual or cached</returns>
        public R GetOrMake<A1, A2, R>(Func<A1, A2, R> Maker, A1 arg1, A2 arg2) { var C = new FuncCallProc<A1, A2, R>(Maker, arg1, arg2); return GetOrMake(C); }

        /// <summary>
        /// Function call cache (3 args version). Usage: <br/>
        /// <code>
        /// MyCache.GetOrMake((a, b, c) => { /* func code */ }, argA, argB, argC);<br/>
        /// // or<br/>
        /// MyCache.GetOrMake(MyFunc, argA, argB, argC);
        /// </code>
        /// </summary>
        /// <typeparam name="R">Function result type (inferred automatically)</typeparam>
        /// <param name="Maker">Function to produce the result</param>
        /// <returns>Function call result ether actual or cached</returns>
        public R GetOrMake<A1, A2, A3, R>(Func<A1, A2, A3, R> Maker, A1 arg1, A2 arg2, A3 arg3) { var C = new FuncCallProc<A1, A2, A3, R>(Maker, arg1, arg2, arg3); return GetOrMake(C); }

        /// <summary>
        /// Function call cache (4 args version). Usage: <br/>
        /// <code>
        /// MyCache.GetOrMake((a, b, c, d) => { /* func code */ }, argA, argB, argC, argD);<br/>
        /// // or<br/>
        /// MyCache.GetOrMake(MyFunc, argA, argB, argC, argD);
        /// </code>
        /// </summary>
        /// <typeparam name="R">Function result type (inferred automatically)</typeparam>
        /// <param name="Maker">Function to produce the result</param>
        /// <returns>Function call result ether actual or cached</returns>
        public R GetOrMake<A1, A2, A3, A4, R>(Func<A1, A2, A3, A4, R> Maker, A1 arg1, A2 arg2, A3 arg3, A4 arg4) { var C = new FuncCallProc<A1, A2, A3, A4, R>(Maker, arg1, arg2, arg3, arg4); return GetOrMake(C); }

        /// <summary>
        /// Function call cache (5 args version). Usage: <br/>
        /// <code>
        /// MyCache.GetOrMake((a, b, c, d, e) => { /* func code */ }, argA, argB, argC, argD, argE);<br/>
        /// // or<br/>
        /// MyCache.GetOrMake(MyFunc, argA, argB, argC, argD, argE);
        /// </code>
        /// </summary>
        /// <typeparam name="R">Function result type (inferred automatically)</typeparam>
        /// <param name="Maker">Function to produce the result</param>
        /// <returns>Function call result ether actual or cached</returns>
        public R GetOrMake<A1, A2, A3, A4, A5, R>(Func<A1, A2, A3, A4, A5, R> Maker, A1 arg1, A2 arg2, A3 arg3, A4 arg4, A5 arg5) { var C = new FuncCallProc<A1, A2, A3, A4, A5, R>(Maker, arg1, arg2, arg3, arg4, arg5); return GetOrMake(C); }
    }
}

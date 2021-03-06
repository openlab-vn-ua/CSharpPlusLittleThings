# CSharpPlusLittleThings

Bunch of cs utilities and useful classes that anyone can use by just drag-in-drop.

Most modules are self-contained, so you may just use single module file.

The toolbox include:

- [ObjectCloner](#ObjectCloner) : Object copy/clone via reflection
- [RangeList](#RangeList) : Drop in replacement of Enumeration.Range but with IList interface
- [RangeAsList](#RangeAsList) : Extension to EnumerationPlus.RangeList for all integral types
- [EmptyList](#EmptyList) : Drop in replacement of Enumeration.Empty but with IList interface
- [FuncCall](#FuncCall) : Deferred function call (to allow argument inspection before call)
- [TickCount64](#TickCount64) : Portable implementation of Environment.TickCount64
- [AppSettingsValue](#AppSettingsValue) : App settings reader using extended Lazy pattern
- [MicroCache](#MicroCache) : Simple in-process function call cache implementation on top of [FuncCall](#FuncCall)

## ObjectCloner
Clone object via reflection.
Useful to clone simple objects that do not implement ICloneable interface
```
var TheClone = ObjectCloner.CreateMemberwiseClone(TheSource);
```
Copy data from one object to another via reflection. 

Useful to copy data between objects with similar structure, but different types
```
ObjectCloner.MemberwiseCopy(TheSource,TheTarget);
```

## RangeList
`EnumerationPlus.RangeList` is drop-in extension for `Enumeration.Range`, but with `IList<int>` interface
* Exports `IList<int>` interface (read only) in addition to `IEnumerable<int>`
* All readers are 100% Thread safe
* All readers and seeker operations are O(1), even IndexOf and Contains
* Just use `EnumerablePlus.RangeList` instead of `Enumerable.Range` // drop-in replacement
```
var SimpleRangeList = EnumerablePlus.RangeList(0,30);
```
 
## RangeAsList
`EnumerationPlus.RangeAsList` is typed extension for our `EnumerationPlus.RangeList` that provides additional benefits:
* Defined for all integer types from sbyte to ulong
* All host type range support for Start parameter (but Count naturally restricted to Int32 by IList itself)
* Suports `RangeAsList(T Count)` additional maker factory function to provide 0..Count-1 elements
```
var TheRangeListOfUInts = EnumerablePlus.RangeAsList((uint)0,10);    // 10 uints with values [0..9]
var SimpleRangeListOfLongs = EnumerablePlus.RangeAsList((long)5,10); // 10 longs with values [5..14]
var AnotherRangeListOfBytes = EnumerablePlus.RangeAsList((byte)30);  // 30 bytes with values [0..29]
```

## EmptyList
`EnumerationPlus.EmptyList` is drop-in extension for `Enumeration.Empty<T>`, but with `IList<T>` interface
* Exports `IList<T>` interface (read only) in addition to `IEnumerable<T>`
* All operations are 100% thread safe
* Just use `EnumerablePlus.EmptyList` instead of `Enumerable.Empty` (drop-in replacement)
```
var SimpleEmptyList = EnumerablePlus.EmptyList<int>();
```
 

## FuncCall
Function call wrapper: Wrapper encloses function + parameters to parameter-less callable object:
```
var MyCall = FuncCall.Create((a, b, c) => { return a + b + c + 1; }, 1, 2, 3)
//...
var MyResult = MyCall.Invoke(); // invoke with args suppled on Create
```
The wrapper creates "deferred" (or wrapped) pending function call. You may use `MyCall.Args` or `MyCall.GetArgsArray()` to inspect arguments given before the call.
This would open way to efficient and simple caching of function calls (eg. you may use args as a source for, say cache key generation, see [MicroCache](#MicroCache))
Then you may call original function with original arguments via:
* `TheFuncCall.Invoke()` : delegate-like call
* `TheFuncCall.Func.Invoke()` : direct closure call
* `TheFuncCall.Call()` : Task like call (returns `FuncCallResult` with `Result` and `Exception` properties)
* `TheFuncCall.GetMakerInfo().Maker.DynamicInvoke(TheFuncCall.GetArgsArray())` : Exotic (dynamic call)
```
var MyCallResult = MyCall.Call(); // invoke with args suppled on Create
if (MyCallResult.IsFaulted) { Console.Error.Write(MyCallResult.Exception); }
else
{
  // Use MyCallResult.Result
}
```
If source function does not take any arguments, content of `Args` is null, else it will be a tuple filled with arguments.
You may use `MyCall.GetMakerInfo()` to check original function properties before the call.
*The parameter-less closue function created is available via `MyCall.Func<ResultType>`.*

## TickCount64

`EnvironmentPlus.TickCount64` is a portable implementation of `Environment.TickCount64`.

It fallbacks to unsigned `Environment.TickCount` if `Environment.TickCount64` is not available on target platform.

## AppSettingsValue

Lazy pattern to extract AppSettingsValue once, with type, default and validation. Usage:
```
// Declare MyParm to extract from app.config MyParamName as integer
int MyParam = new AppSettingsValue<int>("MyParamName"); // with be extracted once

// Declare MyParm2 to extract from app.config MyParam2Name as int with default value of 100 and > 0 requirement
int MyParam2 = new AppSettingsValue<int>("MyParam2Name", 100, (p) => p > 0);

... MyMethod(...)
{
  //...
  if (MyParm.Value > 0)  // your code
  //...
  if (MyParm2.Value > 1000)  // your code
}
```
Default value is used in case there is no value in configuration file or value if invalid (ether cannot be parsed or was rejected by validation function, if any)

## MicroCache

Micro cache is simple function call cache implementation speedup operations inside the process.
* Simple API (just use `GetOrMake` method : `var Result = MyCache.GetOrMake(MyFunc, MyArg1, MyArg2, ...)`)
* Simple operation convention: assumes, that the function and it's explicit arguments are completely defines result
* The function and values of call arguments define cache key (different functions with same parameters may share cache)
* Any number of caches may co-exist in the same process (just instantiate as many `MicroCache` objects as you want
* Supports closures (uses only explicit arguments for keys)
* The data stored inside proccess memory
* Supports cache invalidation
* Supports cache purge to free up resources (can be done by external timer)
* Does not start any threads by itself (purge for expired entries is done at access time)
* Supports multiple expiration policies (you may create custom by override `MicroCache.IsExpiried`)

Usage Example:
```
class MyClass
{
  //Define cache to speedup lengthy operation in scope of this class
  private MicroCache MyCache = new MicroCache();

  //Assume these are function that do some heavy work
  private int CalcComplexStuffImplemenation(int a, int b) // ... some code
  private long CalcAnotherComplexStuffImplemenation(int a, int b) // ... some code

  // You may cache result of operation (different functions may be cached in single cache)
  public int CalcComplexStuff(a, b) { return MyCache.GetOrMake(CalcComplexStuffImplemenation, a, b); }
  public long CalcAnotherComplexStuff(a, b) { return MyCache.GetOrMake(CalcAnotherComplexStuffImplemenation, a, b); }

  // Lambda expression may be used also
  public long CalcMoreComplexStuff(a, b) 
  { 
    return MyCache.GetOrMake((a, b) => 
    { 
      return CalcComplexStuffImplemenation() + CalcAnotherComplexStuffImplemenation();
    }, a, b); 
  }

  // You may invalidate cache in case something changed
  public void UpdateSomeStuff(...)
  {
    // ... some code that implicitely makes cacheed data invalid
    MyCache.Invalidate();
  }

  // You may configure cache expiration policy
  public MyClass(...)
  {
    // You may adjust default expiration polity for items
    MyCache.DefCacheItemAbsoluteExpirationTicksCount = 60*1000; // expiry 1 min from creation
    MyCache.DefCacheItemAccessExpirationTicksCount = 10*1000; // 10 sec from last access
    MyCache.DefCacheItemHitCountExpirationCount = 100; // every 100 hits
    // You may implement more complex expiration logic by overwrite MicroCache.IsExprired method
  }

  // You may purge expired items to free up memory
  public void SomeMemoryCoslyOperation(...)
  {
    MyCache.Purge();
    // ...
  }
}
```

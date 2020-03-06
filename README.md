# CSharpPlusLittleThings

Bunch of cs utilities and usefull classes that anyone can use by just drag-in-drop
The toolbox include:

## EnumerationPlus.RangeList
Drop in extension compatible with Enumeration.Range, but with IList<int> interface
* Exports IList<int> interface (read only) in addition to IEnumerable<int>
* All readers are 100% Thread safe
* All readers and seeker operations are O(1), even IndexOf and Contains
* Just use EnumerablePlus.RangeList instead of Enumerable.Range // drop-in replaceement
 
## EnumerationPlus.RangeAsList
Add-on typed extension for our EnumerationPlus.RangeList that provides additional benefits:
* Defined for all integer types from sbyte to ulong
* All host type range support for Start parameter (but Count naturally restricted to Int32 by IList itself)
* Suports RangeAsList(T Count) additional maker factory function to provide 0..Count-1 elements

## FuncCall
Added function call argument-less clouse abstraction with parameters available externally.
Use this class to encasulate closured function call : the function and its arguments ready to call.
This allow to examine parameters before apply the call (and use them as a source for, say caching key generation)
The call is "suspended" until really need to call.
```
var MyCall = FuncCall.Create((a, b, c) => { return a + b + c+ 1; }, 1, 2, 3)
```
That is, you may use `MyCall.Func<ResultType>` to check function properties and `MyCall.Args` as arguments given before the call. If source function does not take any arguments, content of `Args` is implemenation-defined, else it will be a tuple filled with arguments. You may use `MyCall.Invoke()` to do actual invocation. This would open way to efficient and simple ccing of function calls. Alternatively, you may use `FuncCall.Call()` to obtain `FuncCallResult<ResultType>` object that acts alike `Task` object with `Result` and `Exception` properties.


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

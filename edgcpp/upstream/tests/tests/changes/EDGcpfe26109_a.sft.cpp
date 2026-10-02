//type:fn
//options_all:--c++11
//remark:Enumerations with a bool underlying type
// 6/23/26   [EDGcpfe/26109,EDGcpfe/28827]
//
// Enumerations with a bool underlying type
//
// An enumeration with a fixed underlying type of bool can represent only the
// values false and true.  Two problems with such enumerations have been fixed:
//
// Converting a value to such an enumeration failed to first convert it to
// bool, as required by the standard.
// yields 1 (the value of "(E)true") rather than 42 (as it did previously).
//
// An enumerator whose value is outside the range of bool (here f2, whose
// underlying value would be 2) was not diagnosed.  Now such an out-of-range
// enumerator elicits an error (a warning in Microsoft mode).
enum F : bool { f0, f1, f2 };

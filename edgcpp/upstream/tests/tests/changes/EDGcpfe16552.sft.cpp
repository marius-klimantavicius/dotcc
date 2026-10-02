//type:fn
//options_all:--c++14
//remark:[4.11] Missing error on uses of "operator auto" in C++14 mode
// 10/5/15  [EDGcpfe/16552]
//
// Missing error on uses of "operator auto" in C++14 mode
//
// The front end failed to issue an error on some invalid uses of "operator auto"
// in C++14 mode.  This could lead to internal errors later on or invalid IL.
//
// This is now fixed.
struct S {
  decltype(operator auto) f();  // Previously not diagnosed; now an error.
};

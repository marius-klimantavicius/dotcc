//type:fn
//options_all:--c++14
//remark:[4.12] C++14 constexpr constructors calling non-constexpr subobject constructors
// 7/8/16   [EDGcpfe/17381]
//
// C++14 constexpr constructors calling non-constexpr subobject constructors
//
// In C++14 mode, the front end previously failed to diagnose a constexpr
// constructor directly calling a non-constexpr constructor to initialize one of
// its subobjects (a diagnostic required by the C++14 standard).
//
// This is now fixed.
struct S { S(); };
struct C {
  S s;
  constexpr C(): s() {}  // Previously accepted in C++14 mode.
};                       // Now an error.

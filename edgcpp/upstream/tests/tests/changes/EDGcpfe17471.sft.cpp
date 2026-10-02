//type:fp
//options_all:--c++14
//remark:[4.12] Deduced reference return type with array/function lvalue expression
// 8/18/16  [EDGcpfe/17471]
//
// Deduced reference return type with array/function lvalue expression
//
// The front end incorrectly applied the array-to-pointer and
// function-to-pointer conversions to the expression in a return statement
// when deducing the return type for a function returning a reference type.
// This is now fixed.
auto & f() {
  return "";   // Previously a "must be an lvalue" error, now okay
}
const char (&r)[1] = f();   // Previously a type mismatch, now okay

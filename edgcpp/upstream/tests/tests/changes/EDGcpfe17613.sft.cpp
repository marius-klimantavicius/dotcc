//type:fp
//options_all:--c++14
//remark:[4.13] Array binding failure in C++14 constexpr interpreter
// 10/12/16 [EDGcpfe/17613]
//
// Array binding failure in C++14 constexpr interpreter
//
// The C++14 constexpr interpreter previously failed to handle binding an array
// to a reference to array parameter if the reference includes added type
// qualifiers.
//
// This is now fixed.
using A = int[2];
constexpr bool f(A const&) { return true; }
constexpr bool g() {
  A x = { 1, 2 };
  return f(x);  // The parameter bound to x adds a "const" qualifier.
}               // Previously, this caused interpretation to fail.
constexpr bool r = g();  // Previously triggered an error because the call
                         // to g() did not produce a constant.  Now okay.

//type:fn
//options_all:--clang --c++17
//remark:[5.0] Abort on invalid use of designator in some C++ modes
// 1/25/18  [EDGcpfe/19152]
//
// Abort on invalid use of designator in some C++ modes
//
// In C++ modes that accept both C++11-style initializer lists and C99-style
// designated initializers, the front end could abort on certain invalid uses of
// designators.
//
// This is now fixed: Ordinary errors are issued.
void f() {
  g({ .a = 0 });  // Previously triggered an internal error.
}

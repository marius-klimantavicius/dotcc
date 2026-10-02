//type:fp
//options_all:--c++17 --gn 80000
//remark:[5.1] Nontype template arguments and local static variables
// 5/14/19  [EDGcpfe/21175]
//
// Nontype template arguments and local static variables
//
// In C++17 mode, a nontype template argument of reference or pointer type can now
// refer to a local static variable.
template <int*> struct S {};
auto f() {
  static int x = 0;
  return S<&x>{};  // Now accepted in C++17 mode.
}

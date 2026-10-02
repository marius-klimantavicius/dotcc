//type:fp
//options_all:--c++17
//remark:[5.1] Spurious diagnostics on constexpr and auto static data member definitions
// 10/23/18 [EDGcpfe/19740,EDGcpfe/20329]
//
// Spurious diagnostics on constexpr and auto static data member definitions
//
// The front end previously issued spurious errors on certain static data member
// definitions involving constexpr and/or auto specifiers.
struct S {
  static const int x;
  static int y;
};
constexpr int S::x = 42;  // Previously an error.  Now okay.
auto S::y = 42;  // Previously an error.  Now okay.

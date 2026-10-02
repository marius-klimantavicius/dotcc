//type:fp
//options_all:--c++17
//remark:[5.0] Empty classes and constant expressions
// 2/21/18  [EDGcpfe/18739,EDGcpfe/18901,EDGcpfe/19143,EDGcpfe/19684]
//
// Empty classes and constant expressions
//
// Non-constant variables of empty class types with trivial copy constructors can
// now be copied in constant expressions.
//
// (An error is still issued in Microsoft mode and in some Clang and GNU modes
// for compatibility with the corresponding compilers.)
struct S {};
constexpr int z(S) { return 0; }
void g(S x) {
  constexpr int r = z(x);  // Now permitted.
}

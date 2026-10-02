//type:fp
//remark:[4.4] Abort on nontype template argument that refers to a local static constant
// 10/3/11  [EDGcpfe/11870]
//
// Abort on nontype template argument that refers to a local static constant
//
// In some configurations (particularly those that enable the C++-generating back
// end), the front aborted with an internal error in scope_of_local_variable
// (il.c, "scope not found") when a nontype template argument referred to a
// local static const variable.
//
// This is now fixed.
template<int> void f();
void g() {
  static const unsigned N = 1;
  f<N>();  // Triggered an internal error in some configurations.
}

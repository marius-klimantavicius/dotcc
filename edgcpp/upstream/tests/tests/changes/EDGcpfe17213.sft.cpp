//type:fp
//options_all:--clang
//remark:[4.12] Abort in make_node_from_operand on braced initializer in template
// 5/24/16  [EDGcpfe/17213]
//
// Abort in make_node_from_operand on braced initializer in template
//
// The changes for EDGcpfe/16312 introduced a regression in the handling of
// certain braced initializers in templates (in GNU and Clang mode), causing the
// front end to abort in make_node_from_operand (exprutil.c).
// with "--g++ --no_defer_parse_function_templates":
//
// This is now fixed.
template<typename> void ft();
struct S { void (*pf)(); };
template<typename T> void g() {
  S s = { ft<T> };  // Previously triggered an abort in GNU and Clang
}                   // modes during prototype instantiation.

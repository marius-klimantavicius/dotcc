//type:fp
//options_all:--clang_v 100000 --c++14
//remark:[6.8] C++-generating back end: Abort in gen_expr on enk_initializer node
// 2/25/25  [EDGcpfe/27958,EDGcpfe/27992]
//
// C++-generating back end: Abort in gen_expr on enk_initializer node
//
// The C++-generating back end sometimes aborted with an internal error in
// gen_expr (cp_gen_be.c) on an enk_initializer node (which represents a constant-
// folded dynamic initializer).  This was a consequence of constant-folding
// accidentally introducing a loop in the expression structure of a variable
// initializer.
//
// This regression is now fixed (it was introduced in version 6.7 by the changes
// for EDGcpfe/27456).
struct S {
  constexpr S(const int&) {}
};
template<typename> void g() {
  constexpr S s = S(1);  // Previously triggered an internal error.
}

//type:fn
//options_all:--microsoft_v 1928 --concepts
//remark:[6.2] Abort on requires-expression in nested template constraint
// 11/19/20 [EDGcpfe/23606,EDGcpfe/23607]
//
// Abort on requires-expression in nested template constraint
//
// In C++20 mode, the front end previously sometimes aborted with an internal
// error ("missing default rescan info" in get_expr_rescan_info, exprutil.c) when
// attempting to evaluate a constraint on a nested template written with a
// requires-expression.
//
// This is now fixed.
template<typename T> struct X {
  template<typename U> requires requires { f(U()); }
  X(U u) {}
};
X<int> r{ 1 };  // Previously an internal error.  Now an ordinary error.

//type:fp
//options_all:--gn 130100 --c++20
//remark:[6.6] Abort in substitution of requires-expression in certain contexts
// 9/2/23   [EDGcpfe/26631]
//
// Abort in substitution of requires-expression in certain contexts
//
// This example previously aborted with an internal error in get_expr_rescan_info
// (exprutil.c, "missing default rescan info").  That is now fixed.
template<typename> struct X;
template<> struct X<int> {
  template<typename U> explicit(requires(U p) { p; }) X(U);
};
X<int> x{ 0 };  // Previously aborted.  Now okay.

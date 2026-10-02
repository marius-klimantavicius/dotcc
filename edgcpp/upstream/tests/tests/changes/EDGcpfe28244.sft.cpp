//type:fp
//options_all:--microsoft --ms_c++20
//remark:[6.8] Microsoft mode abort on instantiation of variable template containing a
// 8/28/25  [EDGcpfe/28244]
//
// Microsoft mode abort on instantiation of variable template containing a
// requires-expression
//
// Previously, this aborted with an internal error in get_expr_rescan_info
// (expr.c).  That is now fixed.
template<auto N> constexpr bool vt = requires { N>0; };
static_assert(vt<42>);  // Previously aborted.  Now okay.

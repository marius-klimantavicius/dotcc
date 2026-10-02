//type:fp
//options_all:--gn 70400
//remark:[6.1] Abort in make_cast_rescan_operands
// 1/3/20   [EDGcpfe/22139]
//
// Abort in make_cast_rescan_operands
//
// Under certain circumstances, the front end failed to record some important
// information to enable correct handling of SFINAE for cast expressions.  That
// in turn could result in a failed assertion check in make_cast_rescan_operands
// (exprutil.c).
//
// That problem is now fixed.
template<int, int> struct X {};
template<int A, int B, int C = ((A==1 || B==1) ? 1 : 0)> struct S {};
template <int N, int M> S<N, static_cast<int>(M)> f(X<N, M> const&);
void g() {
  X<3, 3> const y;
  S<3, 3> n = f(y);  // Previously triggered an internal error during the
}                    // substitution of "static_cast<int>(M)".  Now fixed.

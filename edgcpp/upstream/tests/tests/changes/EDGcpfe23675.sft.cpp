//type:fp
//options_all:--c++14 --gnu_version=70500
//remark:[6.2] Abort in unwrap_if_tpck_expression
// 12/15/20 [EDGcpfe/23675]
//
// Abort in unwrap_if_tpck_expression
//
// The changes for EDGcpfe/22825 introduced a regression (in version 6.1) that
// caused the front end to abort (with a null pointer indirection) in
// unwrap_if_tpck_expression (in some configurations).
//
// That is now fixed.
template<typename T> T f(T*);
template<int I> void g(int *x) {
  f<int>(&x[I+1]);  // Triggered an abort in version 6.1.
}

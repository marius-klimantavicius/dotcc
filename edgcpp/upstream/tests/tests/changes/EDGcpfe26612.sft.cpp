//type:fp
//options_all:--gn 110200
//remark:[6.6] Abort on call to deleted member in SFINAE context
// 8/31/23  [EDGcpfe/26612]
//
// Abort on call to deleted member in SFINAE context
//
// This previously triggered an assertion failure in func_call_expr (because an
// error is expected for a call to a deleted function, but in a SFINAE context
// such a diagnostic is inhibited).  That is now fixed.
template<typename T> struct S {
  static auto f(int const&) = delete;
};
template <typename T> decltype(S<T>::f(42)) h(T);
int h(int);
void g() {
  h(43);  // Previously triggered an assertion failure.  Now okay.
}

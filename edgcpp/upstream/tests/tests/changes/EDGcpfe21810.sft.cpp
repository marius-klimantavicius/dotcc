//type:fp
//options_all:--clang
//remark:[6.0] Clang compatibility: __builtin_operator_new and __builtin_operator_delete
// 10/31/19 [EDGcpfe/21810]
//
// Clang compatibility: __builtin_operator_new and __builtin_operator_delete
//
// Clang defines __builtin_operator_new and __builtin_operator_delete builtin
// functions, but their definitions reflect the single argument versions of
// their respective operator functions.  A change has been made to the
// signatures of these builtin functions to also accommodate the overloaded
// signatures (i.e., those with more than a single argument).
// (with --clang):
template <class T1, class T2>
void f(void *ptr, T1 a1, T2 a2) {
  return __builtin_operator_delete(ptr, a1, a2);
}

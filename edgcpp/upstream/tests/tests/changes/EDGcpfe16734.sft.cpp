//type:fp
//options_all:--clang
//remark:[4.11] clang compatibility: __builtin_operator_new and __builtin_operator_delete
// 12/23/15 [EDGcpfe/16734]
//
// clang compatibility: __builtin_operator_new and __builtin_operator_delete
//
// Two new builtins, __builtin_operator_new and __builtin_operator_delete, are
// now available in --clang mode.  An implicit alias is created from these
// builtin routines to the corresponding standard functions so that a back end can
// either use the builtin or the alias as it sees fit.
void f() {
  void *x = __builtin_operator_new((__SIZE_TYPE__)2);
  __builtin_operator_delete(x);
}

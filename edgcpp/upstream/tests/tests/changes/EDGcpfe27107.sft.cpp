//type:fp
//options_all:--clang_v 150000
//remark:[6.7] Clang compatibility: allow floating-point types in some atomic builtins
// 3/19/24  [EDGcpfe/27107]
//
// Clang compatibility: allow floating-point types in some atomic builtins
//
// It appears that Clang versions 13.0.0 and later now allow certain
// floating-point types as the first argument to the __atomic_add_fetch,
// __atomic_sub_fetch, __atomic_fetch_add, and __atomic_fetch_sub builtins.
// The front end now emulates that when clang_version >= 130000.
double f(double* addr, double value) {
  return __atomic_fetch_add(addr, value, 0);
}

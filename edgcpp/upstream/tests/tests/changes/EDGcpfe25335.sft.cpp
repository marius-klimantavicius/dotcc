//type:fp
//options_all:--clang_v 100000
//remark:[6.4] Clang compatibility: __builtin_preserve_access_index
// 7/12/22  [EDGcpfe/25335]
//
// Clang compatibility: __builtin_preserve_access_index
//
// The clang __builtin_preserve_access_index builtin returns the type of its
// first argument -- a change was made to that effect.
// --clang_version 100000:
struct A {
  int m;
  double d;
};
void f(A *ap) {
  int i = __builtin_preserve_access_index(ap->m);
  double d = __builtin_preserve_access_index(ap->d);
}

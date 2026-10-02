//type:fp
//options_all:--clang
//remark:[6.7] GNU/Clang compatibility: array-to-pointer decay on sync builtins
// 7/9/24   [EDGcpfe/27288]
//
// GNU/Clang compatibility: array-to-pointer decay on sync builtins
//
// A missing array-to-pointer decay had caused spurious errors when using an
// array type as the first argument to some GCC/Clang builtins.
// with --clang:
void f() {
  _Atomic(int) i[2];
  __c11_atomic_init(i, 0);
  __c11_atomic_load(i, 0);
}

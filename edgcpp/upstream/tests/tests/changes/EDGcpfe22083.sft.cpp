//type:fp
//options_all:--gnu 70300 -tused
//remark:[6.1] GNU compatibility: template dependent arguments to __builtin_assume_aligned
// 1/17/20  [EDGcpfe/22083]
//
// GNU compatibility: template dependent arguments to __builtin_assume_aligned
//
// A spurious error had been given when a dependent type was used as the third
// argument to __builtin_assume_aligned.  Now fixed.
// --gnu_version 70300):
template <typename T> void f(T *p, T align) {
  __builtin_assume_aligned(p, align, align);
}
void bar(long *p, long align) {
  f(p, align);
}

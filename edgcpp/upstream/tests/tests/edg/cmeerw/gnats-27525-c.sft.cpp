//type:fp
//options:--target linux_i686 --clang_version 190100:--target linux_x86_64 --clang_version 190100:--target linux_armv7 --clang_version 190100:--target linux_aarch64 --clang_version 190100:--target linux_i686 --gn 140200:--target linux_x86_64 --gn 140200:--target linux_armv7 --gn 140200:--target linux_aarch64 --gn 140200
//options_all:--c11 -w

typedef __builtin_va_list __gnuc_va_list;
typedef __gnuc_va_list va_list;

void f(int i, ...)
{
  va_list l;

  __builtin_va_start(l, i);
  int j = __builtin_va_arg(l, int);
  __builtin_vprintf("%s", l);
  __builtin_va_end(l);
}

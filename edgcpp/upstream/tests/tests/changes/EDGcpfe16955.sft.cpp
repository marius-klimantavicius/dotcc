//type:fp
//options_all:--clang
//remark:[4.12] __builtin_addressof
// 5/13/16  [EDGcpfe/16955]
//
// __builtin_addressof
//
// The builtin, __builtin_addressof, has been added in Clang and Microsoft
// emulation modes.  See http://clang.llvm.org/docs/LanguageExtensions.html for
// information.
int fail = 0;
int main() {
  const int ci = 22;
  int i = 33;
  volatile int vi = 44;
  const volatile int cvi = 55;
  if (__builtin_addressof(i) != &i) fail++;
  if (__builtin_addressof(ci) != &ci) fail++;
  if (__builtin_addressof(vi) != &vi) fail++;
  if (__builtin_addressof(cvi) != &cvi) fail++;
  return fail;
}

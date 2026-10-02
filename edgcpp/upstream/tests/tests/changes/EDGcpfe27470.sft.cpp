//type:fp
//options_all:--gcc --c23
//remark:[6.7] Assertion failure in get_token_with_colon_separation
// 7/23/24  [EDGcpfe/27470]
//
// Assertion failure in get_token_with_colon_separation
//
// When parsing some asm constructs in GNU emulation mode, an assertion failure
// (in get_token_with_colon_separation) had occurred in C23 mode.
// (with --gcc --c23):
void f(const void *__config) {
  __asm__ ("ldtilecfg\t%X0" ::"m"(*((const void **)__config)));
}

//type:fp
//options_all:--clang -w
//remark:[6.6] Abort on pragma following attribute
// 7/7/23   [EDGcpfe/26488]
//
// Abort on pragma following attribute
//
// In C++-generating back end configurations this previously triggered an abort
// (null pointer indirection) in function r_set_keep_in_il_on_sslist (il_walk.c).
// That is now fixed.
__attribute__((noreturn))
#pragma GCC push_options
#pragma GCC optimize("O0")
static void f(void) {}

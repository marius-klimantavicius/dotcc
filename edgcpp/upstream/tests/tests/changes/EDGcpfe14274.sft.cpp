//type:fp
//options_all:--microsoft --c99
//remark:[4.8] Microsoft compatibility: __declspec(restrict) in C99 mode
// 7/19/13  [EDGcpfe/14274]
//
// Microsoft compatibility: __declspec(restrict) in C99 mode
//
// In configurations where SUPPRESS_RESTRICT_IN_GENERATED_CODE is TRUE and
// "restrict" is a keyword, using __declspec(restrict) had caused a spurious
// diagnostic which is now fixed.
__declspec(restrict) void *f() {
  return 0;
}

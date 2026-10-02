//type:fp
//options_all:--g++
//remark:[4.10] GNU compatibility: Allow a constant as the second operand of __builtin_va_start
// 8/5/14   [EDGcpfe/15293]
//
// GNU compatibility: Allow a constant as the second operand of __builtin_va_start
//
// In GNU modes, the second operand of __builtin_va_start is now permitted to be
// a constant (a warning is issued) instead of the usual variable.
//
// (GCC appears to generally ignore the second operand of __builtin_va_start.)
void g(int p, ...) {
  __builtin_va_list  ap;
  __builtin_va_start(ap, 42);  // Now accepted with a warning in GNU C++
}                              // mode.

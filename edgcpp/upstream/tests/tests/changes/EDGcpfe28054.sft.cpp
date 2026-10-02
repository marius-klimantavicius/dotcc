//type:fp
//options_all:--g++
//remark:[6.8] GNU compatibility: querying condition code in asm functions now supported
// 3/24/25  [EDGcpfe/28054]
//
// GNU compatibility: querying condition code in asm functions now supported
//
// GNU's asm syntax apparently allows an output to be specified as "=@ccCOND",
// which is a special case of "=" that allows you to query the result of a
// condition code at the end of your assembly statement.  The front end
// now supports this (when GNU_X86_ASM_EXTENSIONS_ALLOWED is TRUE).
// (with --g++):
//
// This code is derived from one of the Boost library headers.
void f(int &i, bool b) {
  __asm__ __volatile__ (
      "lock; incb %[i]\n\t"
      : [i] "+m" (i), [result] "=@ccnz" (b)
      :
      : "memory"
  );
}

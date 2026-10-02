//type:fp
//options_all:--g++
//remark:[5.1] GCC/Clang compatibility: expressions in asm statements
// 9/20/18  [EDGcpfe/20183]
//
// GCC/Clang compatibility: expressions in asm statements
//
// A "::" token that appears as a namespace delimiter in an expression in an asm
// statement had been parsed incorrectly, leading to a spurious error (and in some
// cases, an infinite number of errors).  Now fixed.
int x;
void f() {
   __asm__("" : : "n"(::x));
}

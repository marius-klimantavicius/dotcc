//type:fp
//options_all:--g++
//remark:[5.0] GNU compatibility: lvalue expressions in asm statements
// 8/8/18   [EDGcpfe/19638]
//
// GNU compatibility: lvalue expressions in asm statements
//
// Previously (see the Changes entry for EDGcpfe/11125) the front end had been
// changed to disallow certain lvalue expressions (namely those whose C
// counterpart would be an rvalue).  A change has been made to the front end to
// allow these (in C++ mode) and let lowering deal with the resulting expression.
// Currently only one lvalue expression is supported -- an assignment expression.
// Other expressions will still result in the same error.
// with --g++:
void f(int a, int b) {
  asm volatile ("" : "=r" (b = a));
}

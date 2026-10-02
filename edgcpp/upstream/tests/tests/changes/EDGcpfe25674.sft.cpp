//type:fp
//options_all:--microsoft
//remark:[6.5] Microsoft compatibility: commas in variadic macro arguments
// 12/21/22 [EDGcpfe/25674]
//
// Microsoft compatibility: commas in variadic macro arguments
//
// As described in the entry for EDGcpfe/23821, the traditional Microsoft
// preprocessor usually treats commas appearing in variadic macro arguments as
// ordinary text, not delimiting macro arguments if the variadic argument is
// used as an argument to another macro when the original expansion is
// rescanned.  Under some circumstances, however, such commas do delimit macro
// arguments in a subsequent macro invocation.  The front end previously
// failed to recognize one of the patterns that results in argument-delimiting
// commas, leading to warnings about too few arguments in a macro invocation
// and incorrect macro expansions.  This is now fixed.
// --microsoft -E:
#define M(a,b) b = a
#define M1(x) M
#define M2(args) M1 args
#define M3(...) M2((foo))(__VA_ARGS__)
M3(5, int i);   // Previously expanded to "= 5, int i", now "int i = 5"

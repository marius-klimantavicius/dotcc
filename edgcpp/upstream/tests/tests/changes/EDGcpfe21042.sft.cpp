//type:fp
//options_all:--c --gcc --c11 --gnu_version 70300
//remark:[6.4] Unevaluated subexpressions of type void in GNU C-mode constant expressions
// 9/7/22   [EDGcpfe/21042,EDGcpfe/23680,EDGcpfe/25595]
//
// Unevaluated subexpressions of type void in GNU C-mode constant expressions
//
// Previously, the front end diagnosed an error claiming that the third operand of
// __builtin_choose_expr ("(void)3", which is unevaluated) is not constant.  Now,
// such examples are accepted in GNU modes, to match the behavior of GCC.
char buf[__builtin_choose_expr(1, 2,(void)3)];

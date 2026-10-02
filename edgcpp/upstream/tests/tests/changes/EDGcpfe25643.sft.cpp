//type:fp
//options_all:--gcc --gnu_version=90400
//remark:[6.4] GNU compatibility: Folding of some pseudo-function operands
// 9/16/22  [EDGcpfe/25643]
//
// GNU compatibility: Folding of some pseudo-function operands
//
// The front end previously did not fold some operands of __builtin_constant_p
// and __builtin_choose_expr as aggressively as GCC's front end.  Our
// implementation has been updated to more closely match GCC in this regard.
//
// Previously this failed to compile because the operand of __builtin_constant_p
// did not get folded, which caused that pseudo-function to produce a "false"
// result.  Now, the code is accepted.
void g() {
  typeof((char[__builtin_choose_expr(
                 __builtin_constant_p(
                             1 ?: __builtin_types_compatible_p(int, long)),
                 0,
                 (void)0)]){}) var;
}

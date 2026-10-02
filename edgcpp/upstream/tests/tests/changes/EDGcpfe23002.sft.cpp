//type:fp
//options_all:--c++14 --gnu_version=70300
//remark:[6.2] Lowering of __builtin_constant_p
// 9/15/20  [EDGcpfe/23002,EDGcpfe/23380]
//
// Lowering of __builtin_constant_p
//
// In configurations with DEFAULT_ALWAYS_FOLD_CALLS_TO_BUILTIN_CONSTANT_P set to
// FALSE, the front end sometimes generates actual calls to __builtin_constant_p
// (see the entry of 10/31/06).  However, previously, the operand to such calls
// was parsed as "unevaluated operands" (i.e., like sizeof operands), which caused
// lowering to abort in some cases.
//
// This is now fixed: Such operands are parsed as normal function call operands if
// there is a possibility that they will result in an actual call.
struct A { A(); };
int main() { return __builtin_constant_p(new A); }
  // Previously could abort in lower_init.c in some configurations.

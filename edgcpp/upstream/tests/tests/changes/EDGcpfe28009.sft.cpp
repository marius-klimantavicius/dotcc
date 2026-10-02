//type:fp
//options_all:--gnu=110000
//remark:[6.8] Folding of floating-point builtins had caused undefined behavior
// 3/5/25   [EDGcpfe/28009]
//
// Folding of floating-point builtins had caused undefined behavior
//
// As a result of a missing skip_typerefs, the folding of certain floating-point
// builtin operations could have undefined behavior when operating on a constant
// whose type contains a typeref.
using a = float;
auto b = __builtin_signbit(a());

//type:fp
//options_all:-w --clang_v 220100 --c++23
//remark:Clang compatibility: __builtin_elementwise_ldexp
// 3/12/26  [EDGcpfe/28718]
//
// Clang compatibility: __builtin_elementwise_ldexp, __builtin_masked_load,
// __builtin_masked_expand_load, __builtin_masked_gather
//
// Support has been added for these Clang builtins whose return type is
// dependent on the type of arguments supplied: __builtin_elementwise_ldexp,
// __builtin_masked_load, __builtin_masked_expand_load, __builtin_masked_gather.
float f(float x, int e) { return __builtin_elementwise_ldexp(x, e); }

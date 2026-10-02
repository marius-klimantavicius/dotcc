//type:fp
//options_all:--clang_v 30500
//remark:[6.2] Clang compatibility: function multiversioning with clang_version < 30700
// 8/4/20   [EDGcpfe/23198]
//
// Clang compatibility: function multiversioning with clang_version < 30700
//
// In configurations that use clang compatibility with clang_version < 30700,
// that also have USE_X86_FUNCTION_MULTIVERSIONING and DO_IL_LOWERING,
// using the function multiversioning feature (i.e., with "target" attributes)
// had resulted in an assertion failure in load_matching_builtin_function.
// Now fixed.
__attribute__ ((target("avx2"))) void f();

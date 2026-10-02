//type:fn
//options_all:--c++ --clang_v 210100 --c++ --clang_v 210100
//remark:Clang compatibility: Abort on __builtin_invoke
// 12/15/25 [EDGcpfe/28595]
//
// Clang compatibility: Abort on __builtin_invoke
//
// Previously, the front end could abort with a failed assertion in
// conv_expr_function_designator_to_ptr_to_function when applying __builtin_invoke
// to a constexpr function call operator template or a generic lambda.  For
// example, with --c++ --clang_version 210100:
//
// This change also fixes an abort due to a null pointer indirection when
// attempting to use __builtin_invoke with too few arguments.
// --c++ --clang_version 210100:
int j = __builtin_invoke();  // Previously segfaulted.  Now an error.

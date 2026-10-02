//type:fp
//options_all:--clang
//remark:[6.0] Clang compatibility: __builtin_*_overflow builtins
// 7/16/19  [EDGcpfe/18875,EDGcpfe/19127,EDGcpfe/21547]
//
// Clang compatibility: __builtin_*_overflow builtins
//
// The clang versions of __builtin_add_overflow, __builtin_mul_overflow, and
// __builtin_sub_overflow are "generic" in that they accept three arguments of
// various types and require the compiler to validate the argument types.  The
// (dummy) signature for these builtins (as reported by clang) is "void (...)" and
// has now been changed to "__edg_bool_type__ (...)" so the return type is
// correct.
//
// As a consequence of this change, user-defined entries for builtins (i.e.,
// in builtin_user_table) will now take the place of similarly-named builtins
// in the generic builtin table (i.e., builtin_table).  This allows customers to
// change entries that are found to be incorrect in builtin_table.
void f() {
  unsigned long result;
  if (__builtin_add_overflow(1, 1, &result) ||
      __builtin_mul_overflow(1, 1, &result) ||
      __builtin_sub_overflow(1, 1, &result)) {
  }
}

//type:fn
//options_all:--clang
//remark:[4.13] Clang compatibility: Invalid type specifier combinations
// 12/16/16 [EDGcpfe/17869]
//
// Clang compatibility: Invalid type specifier combinations
//
// Clang mode previously inadvertently failed to diagnose certain invalid type
// specifier combinations (corresponding to GCC-specific behavior).
//
// This is now fixed.
# 1 "syshdr" 3 4
typedef int wchar_t;   // Previously accepted in Clang mode.  Now an error.

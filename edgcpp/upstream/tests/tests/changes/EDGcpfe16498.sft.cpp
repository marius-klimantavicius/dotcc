//type:fp
//options_all:--clang
//remark:[4.11] Abort in Clang mode on context-sensitive __is_... keyword
// 9/4/15   [EDGcpfe/16498]
//
// Abort in Clang mode on context-sensitive __is_... keyword
//
// The Clang-mode changes for EDGcpfe/14876,EDGcpfe/14934 (see entry of 3/21/14)
// could lead to an internal error (in scan_unary_type_trait_helper) when a
// context-sensitive __is_... keyword did not correspond to a unary trait (e.g.,
// when it corresponds to a binary trait).
//
// This is now fixed.
struct __is_base_of {};
  // __is_base_of is now a context-sensitive keyword.
bool r = __is_base_of(int, int);
  // Previously triggered an internal error; now okay.

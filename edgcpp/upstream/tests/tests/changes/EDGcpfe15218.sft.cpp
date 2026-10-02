//type:fp
//options_all:--microsoft
//remark:[4.10] Abort on __is_literal_type in Microsoft C++ mode
// 6/27/14  [EDGcpfe/15218]
//
// Abort on __is_literal_type in Microsoft C++ mode
//
// The front end frequently aborted with an internal error in class_decl.c
// (check_if_constexpr_generated_default_constructor) when processing the
// __is_literal_type predicate for a non-aggregate class type (unless constexpr
// was enabled).
//
// This is now fixed.
struct S { S(int); };
bool const b = __is_literal_type(S);  // Previously aborted in some
                                      // Microsoft C++ modes.  Now okay.

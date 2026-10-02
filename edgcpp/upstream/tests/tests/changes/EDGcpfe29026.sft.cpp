//type:fp
//options_all:--c++26 --gnu=160100
// 8/27/26  [EDGcpfe/29026]
//
// GNU compatibility: __builtin_constexpr_diag
//
// The GNU builtin __builtin_constexpr_diag, which is GCC 16's intrinsic for the
// facility proposed in C++ paper P2758R5 to let constant evaluation produce
// diagnostics, is now supported.  Its three arguments are a severity, a tag
// naming the diagnostic, and the text to be reported.
// --c++26 --gnu=160100:
//
// A severity of 0 produces a remark, 1 a warning, and 2 an error.  Adding 16 to
// the severity asks that the diagnostic be reported at the call of the routine
// containing the call, which is what wrappers such as the proposed
// std::constexpr_error_str would want.  The tag, which is omitted from the output
// when it is empty, and the text may each be given as a pointer to a string
// literal or as an object with the representation of std::string_view.
//
// The new --constexpr_diag_suppress, --constexpr_diag_remark,
// --constexpr_diag_warning, and --constexpr_diag_error options each take a comma
// separated list of tags and give the severity to be used for the diagnostics
// requested with those tags; --constexpr_diag_suppress asks that no diagnostic be
// produced.  This is the mechanism that P2758 recommends implementations provide
// so that a library can let its users control the diagnostics it produces.  For
// the example above, --constexpr_diag_error=negative makes the warning an error.
constexpr int f(int i) {
  if (i < 0) __builtin_constexpr_diag(1, "negative", "a negative argument");
  return i;
}
constexpr int j = f(-1);

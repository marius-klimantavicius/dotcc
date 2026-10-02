//type:fn
//options_all:--microsoft_version=1800
//remark:[4.11] Internal error on use of C++11 features in some C++03 modes
// 8/27/15  [EDGcpfe/16475]
//
// Internal error on use of C++11 features in some C++03 modes
//
// In some GNU, Clang, and Microsoft modes that do not enable C++11 mode (but that
// do accept "defaulted" functions), the front end sometimes aborted with an
// internal error in resolve_indeterminate_exception_specification (class_decl.c).
// --microsoft_version=1800:
//
// This is now fixed.
struct S  {
  S() = default;
  constexpr S(S const&);
};
constexpr S::S(S const&) = default;  // Previously triggered an internal
                                     // error in some modes.

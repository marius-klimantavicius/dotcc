//type:fp
//options_all:--c++17
//remark:[6.4] Assertion failure with raw literal operator in a template
// 1/22/22  [EDGcpfe/25025]
//
// Assertion failure with raw literal operator in a template
//
// In configurations that create the string form of template definitions, the
// changes for EDGcpfe/19745 in version 5.0 resulted in an assertion failure
// in add_token_to_string when a template definition contains a user-defined
// literal that refers to a raw literal operator (i.e., one with a single
// const char* parameter).  This is now fixed.
int operator ""_xx(const char *);
template<typename> void f() {
  5_xx;  // Previously an assertion failure
}

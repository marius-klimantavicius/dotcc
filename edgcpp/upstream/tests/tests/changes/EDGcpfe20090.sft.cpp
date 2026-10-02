//type:fp
//options_all:--c++11
//remark:[5.1] Assertion failure with UDL using literal operator template
// 9/14/18  [EDGcpfe/20090,EDGcpfe/20129]
//
// Assertion failure with UDL using literal operator template
//
// In configurations that create the string form of template definitions, the
// changes for EDGcpfe/19745 in version 5.0 resulted in an assertion failure
// in add_token_to_string (literal.c) when a template definition contains a
// user-defined literal that refers to a literal operator template.  This is
// now fixed.
template<char... Digits> int operator""_s() { return 0; }
template<typename> void f() {
  int i = 10_s;   // Previously aborted
}

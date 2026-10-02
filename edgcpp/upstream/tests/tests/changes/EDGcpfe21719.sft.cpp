//type:fp
//options_all:--no_const_string_literals --c++11
//remark:[6.0] C++-generating back end: assertion failure with user-defined literal
// 9/3/19   [EDGcpfe/21719]
//
// C++-generating back end: assertion failure with user-defined literal
//
// In C++-generating back end configurations with non-const string literals
// (either by setting the configuration macro DEFAULT_STRING_LITERALS_ARE_CONST
// to FALSE or via the command-line option --no_const_string_literals), an
// invocation of a raw literal operator (i.e., one with a const char *
// parameter) resulted in an assertion failure in handle_operator_call.  This
// is now fixed.
 unsigned operator "" _w(const char*);
 void f() {
   12_w;  // Previously an assertion failure, now okay
 }

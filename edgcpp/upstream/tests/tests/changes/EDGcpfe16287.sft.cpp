//type:fp
//options_all:--c++03
//remark:[4.11] C++-generating back end: assertion failure with parenthesized initializer
// 7/20/15  [EDGcpfe/16287]
//
// C++-generating back end: assertion failure with parenthesized initializer
//
// The C++-generating back end aborted with a failed assertion in
// gen_paren_or_brace_dynamic_init in pre-C++11 modes when a variable is
// direct-initialized with an explicit temporary created via a call to the
// default constructor of a class.  This is now fixed.
// --c++03:
struct X {
  X();
};
void f() {
  X a((X()));   // Previously aborted, now okay
}

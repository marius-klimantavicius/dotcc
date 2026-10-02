//type:fp
//options_all:--c++11
//remark:[6.4] C++-generating back end: assertion failure with lambda variable
// 5/23/22  [EDGcpfe/25166]
//
// C++-generating back end: assertion failure with lambda variable
//
// The C++-generating back end sometimes aborted with an assertion failure in
// gen_paren_or_brace_dynamic_init when a variable is initialized with a
// lambda expression.  This is now fixed.
void f() {
  int i;
  auto l{[&i]{}};  // Previously aborted
}

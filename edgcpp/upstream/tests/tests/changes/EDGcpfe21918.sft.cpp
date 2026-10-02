//type:fp
//options_all:--g++
//remark:[6.0] C++-generating back end: abort with non-dependent reference to incomplete class
// 11/12/19 [EDGcpfe/21918]
//
// C++-generating back end: abort with non-dependent reference to incomplete class
//
// The C++-generating back end could abort with an assertion failure in
// push_class_name_context when a template definition contains a non-dependent
// reference to a member of an incomplete class (which is permitted in g++
// emulation mode).  This is now fixed.
struct S;
template <typename T> void f(S &s) {
  s.x();   // S is incomplete; previously aborted, now okay
}

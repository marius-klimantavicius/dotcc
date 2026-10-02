//type:fp
//remark:[4.4] Incorrect source sequence entries list for repeated using-declaration
// 10/27/11 [EDGcpfe/12344]
//
// Incorrect source sequence entries list for repeated using-declaration
//
// When a using-declaration referring to an overload set in a namespace scope was
// repeated, the resulting source sequence entries list (in configurations with
// GENERATE_SOURCE_SEQUENCE_LISTS set to TRUE) was incorrect; specifically, it
// would contain too many entries for using-declarations.  If the repeated using-
// declarations appeared in a local scope and were followed by an additional
// statement, the invalid list triggered an internal error in the C++-generating
// back end ("wrong entry").
//
// This is now fixed.
namespace N {
  void f();
  void f(int);
}
void g() {
  using N::f;
  using N::f; // Previously incorrectly represented in some configurations.
  return;     // Previously triggered an internal error in the
}             // C++-generating back end.

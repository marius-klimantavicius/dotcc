//type:fn
//options_all:--c++14
//remark:[4.13] Assertion failure in find_linked_symbol on invalid code
// 1/4/17   [EDGcpfe/17910]
//
// Assertion failure in find_linked_symbol on invalid code
//
// An assertion failure in find_linked_symbol had occurred if a namespace and
// a function definition were given the same name in an inline namespace.
// Now fixed.
inline namespace {
  namespace B {}
  void B() {}
}

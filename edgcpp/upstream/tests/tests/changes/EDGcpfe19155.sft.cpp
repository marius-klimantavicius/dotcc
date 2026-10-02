//type:fn
//options_all:--c++11
//remark:[5.0] Assertion failure with attribute with no name
// 1/18/18  [EDGcpfe/19155]
//
// Assertion failure with attribute with no name
//
// In error cases where an attribute has not been given a name, error reporting
// mechanisms had aborted (in process_fill_in) when attempting to produce
// an error.  Now fixed.
struct A {};
struct B {
  friend struct [[]] A;
};

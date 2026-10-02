//type:fn
//options_all:--c++20 --strict
//remark:[6.6] Constant-destruction and mutable members
// 10/6/23  [EDGcpfe/26663]
//
// Constant-destruction and mutable members
//
// This example is invalid because the destructor for s, which must implement
// constant-destruction since s is a constexpr variable, accesses a mutable
// member (whose value could conceivably change at run time between its
// initialization and destruction).  The front end previously failed to diagnose
// this.  That is now fixed.
struct S {
  mutable int x = 1;
  constexpr ~S() { auto r = this->x; }
};
constexpr S s;  // Previously accepted.  Now an error.

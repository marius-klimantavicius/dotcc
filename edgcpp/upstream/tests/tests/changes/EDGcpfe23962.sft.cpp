//type:fp
//options_all:--gn 70300 --c++14 -W
//remark:[6.4] Member functions in unnamed namespaces referenced in unevaluated contexts
// 3/17/22  [EDGcpfe/23962]
//
// Member functions in unnamed namespaces referenced in unevaluated contexts
//
// The front end previously issued a "declared but never referenced" warning
// for class member functions (but not for nonmember functions) declared in an
// unnamed namespace and referenced only in an unevaluated context.  This is
// now fixed, and member functions are treated like nonmember functions with
// respect to these warnings.
namespace {
struct S {
  void f() { }
};
typedef decltype(S().f()) v;  // Previously did not suppress the "never
                              // referenced" warning for S::f
}

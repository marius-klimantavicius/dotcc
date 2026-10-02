//type:fp
//options_all:--c++20
//remark:[6.6] Requires-expressions in friend function template constraints
// 7/18/23  [EDGcpfe/25157]
//
// Requires-expressions in friend function template constraints
//
// Previously, the front end did not correctly substitute a requires-expression in
// a friend function template constraint, resulting in the constraint not being
// considered unsatisfied.
template<class T> struct A {
  template<typename U> requires requires { typename U::type; }
  friend void f(A, U) { }  // #1
};
int f(A<int>, long);       // #2
int i = f(A<int>(), 1);    // Previously a spurious error as #1 was called.
                           // Now calls #2.

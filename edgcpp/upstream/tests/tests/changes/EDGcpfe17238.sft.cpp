//type:fn
//options_all:--c++11
//remark:[4.12] Internal error on use of carries_dependency in friend declaration
// 6/7/16   [EDGcpfe/17238,EDGcpfe/17258]
//
// Internal error on use of carries_dependency in friend declaration
//
// The use of a carries_dependency attribute on a parameter in a friend
// declaration where no carries_dependency attribute appeared in the first
// declaration had resulted in an internal error (in
// check_carries_dependency_for_params) rather than an error.  Now fixed.
struct S {
  void f(int);
};
struct A {
  friend void S::f([[carries_dependency]] int);
};

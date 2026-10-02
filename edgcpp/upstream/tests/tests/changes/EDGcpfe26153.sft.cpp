//type:fp
//options_all:--clang_v 70500
//remark:[6.6] Assertion failure in unscan_attributes
// 9/13/23  [EDGcpfe/26153,EDGcpfe/26653]
//
// Assertion failure in unscan_attributes
//
// An assertion failure in unscan_attributes had occurred in some Clang emulation
// modes as a result of the changes for EDGcpfe/25930 (in version 6.5).  That
// is now fixed.
template <typename T> struct A {
  struct X {};
  void f() __attribute__((warn_unused_result)) {}
};
struct B {
  __attribute__((visibility("default"))) A<int>::X Foo();
};

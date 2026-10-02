//type:fp
//options_all:--c++14 --g++
//remark:[4.13] Assertion failure when attribute is applied to variable template
// 10/13/16 [EDGcpfe/17621]
//
// Assertion failure when attribute is applied to variable template
//
// An assertion failure (in get_attribute_link) had occurred in cases where
// an attribute is applied to a variable template.  Now fixed.
// (with --c++14 --g++):
struct A {
  template <typename T> static __attribute__((aligned(2))) T m;
};

//type:fp
//options_all:--c++14
//remark:[4.10] Assertion failure processing aggregate initializer
// 12/16/14 [EDGcpfe/15834]
//
// Assertion failure processing aggregate initializer
//
// The front end aborted with an assertion failure in do_fadd for certain
// aggregate initializers in which the initializer for one field depends on
// the value of a preceding field.  This is now fixed.
// --c++14:
struct A {
  double x = x++;
  int z = x + 37;
};
static const A a = { };   // Previously aborted

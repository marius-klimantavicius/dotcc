//type:fp
//options_all:--c++11
//remark:[4.5] Abort on attempt to convert pointer-to-member selection x.*y back to lvalue
// 5/2/12   [EDGcpfe/12882]
//
// Abort on attempt to convert pointer-to-member selection x.*y back to lvalue
//
// The front end aborted in assign_expr_to_temp ("temp of class type with cctor")
// or conv_class_rvalue_operand_to_lvalue ("couldn't convert to ptr") during
// IL lowering on an attempt to convert a pointer-to-member selection
// using ".*" whose value is an rvalue (because its left operand is
// an rvalue) back to an lvalue so its address can be taken.  Now fixed.
struct A {
  A() {}
  A(A&&) {}
};
struct B {
  A a;
};
B f();
typedef A B::*pd;
pd p = &B::a;
A a1 ((f().*p)) ;

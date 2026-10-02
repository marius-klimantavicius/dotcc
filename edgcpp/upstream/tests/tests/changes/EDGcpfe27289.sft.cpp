//type:fp
//options_all:--c++14
//remark:[6.7] Spurious error on assignment with dependent class member access expression
// 7/4/24   [EDGcpfe/27289]
//
// Spurious error on assignment with dependent class member access expression
//
// Because the left operand of the class member access expression is still
// type-dependent at template definition time, its value category can't be
// determined yet.  However, the front end previously treated the class member
// access expression as a non-dependent xvalue of type "int", thereby eliciting an
// error for using an xvalue on the left-hand side of an assignment operator.
template<typename T> T g();
template<typename T> struct A {
  int m;
  int f() {
    return g<A &>().A::m = 1;  // Previously a spurious error.  Now okay.
  }
};
int i = A<int>().f();

//type:fp
//options_all:--c++11
//remark:[4.12] Lowering of aggregate in conditional operator leads to assertion failure
// 7/5/16   [EDGcpfe/17317]
//
// Lowering of aggregate in conditional operator leads to assertion failure
//
// The lowering of certain conditional operators (where the second and third
// operands return a class prvalue) could produce an assertion failure (in
// lower_dynamic_init) when one or more of the operands contained aggregates
// whose lowered form included keeping a constant value.  That has now been
// fixed.
struct A { int val[2]; } a;
void f(bool b, int in) {
  a = b ? A{{in,2}} : A{{3,in}};
}

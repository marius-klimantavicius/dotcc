//type:fp
//options_all:--gcc --gnu=140100
//remark:[6.7] C-generating back end: internal error on small floating point type and ellipsis
// 5/16/24  [EDGcpfe/27259]
//
// C-generating back end: internal error on small floating point type and ellipsis
//
// The C-generating back end incorrectly assumed all floating point types
// shorter than double should be widened to double when passed to an ellipsis
// and issued an internal error when it encountered an unwidened type in an
// argument expression.  In fact, widening only applies to the float type and
// not to other small floating point types.  This is now fixed.
// with --gcc --gnu_version=140100:
void f(int, ...);
void g() {
  __bf16 x;
  f(0, x);   // Previously caused an internal error
}

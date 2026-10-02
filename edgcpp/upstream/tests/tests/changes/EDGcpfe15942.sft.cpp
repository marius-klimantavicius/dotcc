//type:fp
//options_all:--gcc
//remark:[4.10.1] Lowering of multidimensional arrays with qualified element types
// 1/29/15  [EDGcpfe/15942]
//
// Lowering of multidimensional arrays with qualified element types
//
// A regression (introduced in 4.10) caused an assertion failure (in
// handle_multidimensional_ck_init_repeat) when lowering a repeated aggregate
// constant with an underlying qualified element type.  Now fixed.
// (with --gcc):
const char x[16][2] = {
   [0 ... 15] = "xy"
};

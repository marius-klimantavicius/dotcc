//type:fp
//options_all:--gcc
//remark:[4.10.1] Assertion failure when lowering initialization of multi-dimensional array
// 3/25/15  [EDGcpfe/16111]
//
// Assertion failure when lowering initialization of multi-dimensional array
//
// Using a character string to initialize a multi-dimensional character array
// using repeated designated initializers had resulted in an assertion failure (in
// handle_multidimensional_ck_init_repeat).  This regression was introduced in
// 4.10, but the example below had resulted in an infinite loop in C-generating
// configurations prior to 4.10.  Both issues are now fixed.
// with --gcc:
const char x[2][4] = {
  [0 ... 1] = ""
};

//type:fp
//options_all:--c++14 --gnu_version 70400
//remark:[6.0] Internal error in conv_glvalue_expr_to_prvalue in template
// 8/7/19   [EDGcpfe/21403]
//
// Internal error in conv_glvalue_expr_to_prvalue in template
//
// In some configurations and C++-language modes, the front end could abort with
// an internal error in conv_glvalue_expr_to_prvalue (exprutil.c; with message
// "bad expr") on some expressions.
// gnu_version=70000 and a configuration with the C++-generating back end:
//
// That problem is now fixed.
template<int N> void g(float *p, float d) {
  float r = (N == 1) ? p[0] : d;
    // Previously triggered an internal error.  Now okay.
}

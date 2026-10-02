//type:fp
//options_all:--microsoft_v 1938 --ms_c++latest
//remark:[6.5] Abort on capturing of explicit-this parameter
// 11/2/22  [EDGcpfe/25776]
//
// Abort on capturing of explicit-this parameter
//
// Previously, this triggered an internal error in this_param_value_expr (il.c).
// That is now fixed.
void f() {
  bool b = false;
  auto lm = [&](this auto&&) {
    [&](auto&&) { return b; };  // Previously triggered an internal error.
  };                            // Now okay.
}

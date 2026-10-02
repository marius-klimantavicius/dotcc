//type:fp
//options_all:--c++14
//remark:[4.11] Internal error on init-capture following capture of "this"
// 8/11/15  [EDGcpfe/16421]
//
// Internal error on init-capture following capture of "this"
//
// In C++14 mode (and other modes that support init-capture in lambdas) the front
// end sometimes aborted with an internal error when an init-capture follows the
// capture of "this" (the abort was the result of invoking expect_error in
// find_lambda_capture).
//
// This is now fixed.
struct S {
  int m;
  int f() {
    return [this, c = m]() { return c; }();
  }
};

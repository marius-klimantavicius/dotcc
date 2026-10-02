//type:fp
//remark:[5.1] Abort when capturing "this" in a nested lambda for a field initializer
// 7/10/19  [EDGcpfe/21505]
//
// Abort when capturing "this" in a nested lambda for a field initializer
//
// When a nested lambda in a field initializer captured "this" and the enclosing
// lambda is a generic lambda, the front end would abort with an assertion failure
// in scan_lambda_capture_list.
//
// This is now fixed.
struct A {
  int x = [=](auto a) {
    [this]{}; // Previously caused an abort
    return 0;
  }(0);
};

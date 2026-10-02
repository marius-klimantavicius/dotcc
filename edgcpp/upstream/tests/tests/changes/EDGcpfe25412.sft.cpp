//type:fp
//options_all:--g++
//remark:[6.4] Assertion failure in deferred_check_unused_result_attr
// 7/12/22  [EDGcpfe/25412]
//
// Assertion failure in deferred_check_unused_result_attr
//
// Use of the "warn_unused_result" attribute on a typedef had caused an assertion
// failure in deferred_check_unused_result_attr and is now fixed.
// with --g++:
typedef void *(__attribute__((warn_unused_result)) *F)();

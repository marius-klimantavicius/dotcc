//type:fp
//options_all:--g++
//remark:[4.10] GNU compatibility: Segfault in apply_warn_unused_result_attr
// 6/13/14  [EDGcpfe/15228]
//
// GNU compatibility: Segfault in apply_warn_unused_result_attr
//
// In a case like the one given below where the GNU __warn_unused_result__ is
// applied to the function type by the use of parentheses, a segfault had
// occurred in apply_warn_unused_result_attr and is now fixed.
// (with --g++):
int (__attribute__((__warn_unused_result__)) f)();

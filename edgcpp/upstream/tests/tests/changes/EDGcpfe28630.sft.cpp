//type:fp
//options_all:--g++
//remark:Assertion failure on zero-length array initialization
// 1/12/26  [EDGcpfe/28630]
//
// Assertion failure on zero-length array initialization
//
// In configurations that use lowering, the lowering of an array with zero
// elements had, in some cases, caused an assertion failure in
// lower_dynamic_init_aggregate_constant.  This regression was introduced in
// version 6.8 (by the changes for EDGcpfe/28106) and is now fixed.
// with --g++:
struct A {
 A();
} a[0];

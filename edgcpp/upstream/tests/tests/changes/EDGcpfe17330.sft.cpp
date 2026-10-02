//type:fp
//options_all:--c++11
//remark:[4.12] Abort on trivial value-initialization of array member
// 6/21/16  [EDGcpfe/17330]
//
// Abort on trivial value-initialization of array member
//
// In C++11 mode, a mem-initializer performing value-initialization of an array
// whose element type has a trivial defaulted constructor triggered an internal
// error in scan_parenthesized_mem_init_args (decl_inits.c).
//
// This is now fixed.
struct S {
  S() = default;
};
struct X {
  X(): arr() {}  // Previously triggered an assertion failure; now okay.
  S arr[1];
};

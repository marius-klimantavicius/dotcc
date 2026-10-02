//type:fp
//options_all:--g++
//remark:[4.13] _Pragma use in functions could cause assertion failure
// 10/19/16 [EDGcpfe/16990,EDGcpfe/17648]
//
// _Pragma use in functions could cause assertion failure
//
// In C++-generating configurations, the use of an immediate _Pragma in a function
// would lead to an assertion failure (alloc_copy_of_pending_pragma: copied pragma
// has source sequence entry).  Now fixed.
struct A {
  void f() {
    _Pragma ("GCC diagnostic push")
    _Pragma ("GCC diagnostic pop")
  }
};

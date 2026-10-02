//type:fp
//options_all:--c++14
//remark:[4.10] Issue with lowering of certain multi-dimensional arrays
// 11/24/14 [EDGcpfe/15722]
//
// Issue with lowering of certain multi-dimensional arrays
//
// An assertion failure ("lower_aggregate_designated_initializers: type mismatch")
// had occurred in certain circumstances (involving designated initializers or
// anonymous unions with initialized fields and multi-dimensional arrays).  The IL
// allows for a multi-dimensional aggregate array to be initialized with a single
// ck_init_repeat construct, but lowering had expected such a construct to
// initialize the entire multi-dimensional aggregate, and not just a piece of it.
// Lowering has been changed to handle this case.
struct A {
  struct B {
    int x = 37;
  } a[2][3];
  union {
    char y;
    int x = 38;
  };
} a = { 19 };

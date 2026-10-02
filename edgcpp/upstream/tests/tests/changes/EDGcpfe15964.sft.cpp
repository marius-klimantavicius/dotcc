//type:fp
//options_all:--c++14
//remark:[4.10.1] Assertion failures in lowering during initialization of aggregate array
// 2/5/15   [EDGcpfe/15964,EDGcpfe/15965]
//
// Assertion failures in lowering during initialization of aggregate array
//
// Various assertion failures ("lower_dynamic_init_aggregate_constant: repeat on
// non-array", "lower_dynamic_init_aggregate_constant: bad aggr kind",
// "lower_dynamic_init_aggregate_constant: have constant, no field") had occurred
// when lowering repeated dynamic initialization of certain aggregates in C++14
// mode.  Now fixed.
struct A {
  struct B {
    int y = f();
    int f() {return 10;}
  };
  B b{};
};
A a[2]{};

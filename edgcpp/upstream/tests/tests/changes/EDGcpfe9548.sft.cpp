//type:fp
//options_all:--g++
//remark:[4.1] Assertion failure on initialization of aggregate with one destructible entity
// 5/12/09 [EDGcpfe/9548]
//
// Assertion failure on initialization of aggregate with one destructible entity
//
// In configurations where exception handling is enabled, the initialization
// of an aggregate with exactly one entity where that entity requires a
// destruction could have resulted in an assertion failure
// ("add_dyn_init_cleanup: no temps") in cases involving overlapping object
// lifetimes.
struct A {
  ~A() {}
};
struct B {
  A a;
};
void f() {
  const B& b = (B){ A() };
}

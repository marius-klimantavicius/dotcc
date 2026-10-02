//type:fp
//options_all:--c++17
//remark:[5.1] Assertion failure in lower_param_ref
// 11/27/18 [EDGcpfe/20484,EDGcpfe/20487]
//
// Assertion failure in lower_param_ref
//
// As a result of the C++17 changes to allow aggregates to have base classes in
// 5.0 (see EDGcpfe/17688), some aggregate initialization that had implicitly
// referred to "this" could fail an assertion check (in lower_param_ref).
struct B {};
struct A : B {
  int x;
  struct C : B {} c {(struct A&&)x};
} a{};

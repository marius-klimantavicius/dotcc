//type:fp
//options_all:--c++14
//remark:[4.12] Abort on generic lambda in range-based "for" loop
// 9/27/16  [EDGcpfe/17300]
//
// Abort on generic lambda in range-based "for" loop
//
// In C++14 mode, instantiating a generic lambda used in the body for a range-
// based "for" loop could trigger an internal error in pop_scope_full (saying
// "unexpected curr_object_lifetime for function or block scope") in some
// configurations.
//
// This example previously triggered the abort because S::f instantiates the
// generic lambda passed in from within the range-based "for" loop in function g.
// This is now fixed.
struct S { template<class F> void f(F p) { p(42); } };
struct I {
  ~I();
  S operator*() const;
  void operator++();
  bool operator!=(I const&) const;
};
struct C {
  I begin(), end();
};
void g() {
  for (auto s : C()) s.f([](auto){});
}

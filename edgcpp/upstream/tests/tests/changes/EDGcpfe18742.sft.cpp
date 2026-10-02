//type:fp
//options_all:--c++14
//remark:[5.0] Abort on use of implicit "this" in generic lambda
// 10/10/17 [EDGcpfe/18742]
//
// Abort on use of implicit "this" in generic lambda
//
// In some cases, the front end aborted in make_field_for_lambda_capture (due to
// a null pointer dereference) during the instantiation of a generic lambda whose
// body contains a call to a member function template that relies on an implicit
// "this" selector.
//
// This is now fixed.
struct S {
  template<typename T> void ft(T);
  void f();
};
void S::f() {
  auto lm = [&](auto p) { ft(p); };
  lm(0);
}

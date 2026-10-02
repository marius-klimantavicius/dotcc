//type:fp
//options_all:--c++20
//remark:[6.2] consteval calls in unevaluated contexts
// 9/16/20  [EDGcpfe/22901]
//
// consteval calls in unevaluated contexts
//
// The front end now implements the changes of the C++ standardization committee's
// paper P1937R2, which cause consteval calls in non-evaluated contexts to no
// longer be called.
//
// Previously, this elicited an error because the evaluation of g(f()) could not
// be completed due to a missing definition of f().  Now the example is accepted
// because g(f()) is no longer evaluated.
constexpr int f();
consteval int g(int p) { return p; }
decltype(g(f())) r = 42;  // Previously an error.  Now okay.

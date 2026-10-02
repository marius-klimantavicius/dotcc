//type:fp
//options_all:--c++20
//remark:[6.2] Abort in check_template_constraints
// 10/7/20  [EDGcpfe/23445]
//
// Abort in check_template_constraints
//
// The front end previously sometimes aborted when checking type-constraints on
// certain templates (in check_template_constraints, attempting to dereference a
// null pointer).
//
// That is now fixed.
template<typename T> concept C = requires(T &r) { r.f(); };
template<typename T> struct S { template<typename U> S(U&&); };
template<C T> S(T&&) -> S<int>;
struct X { int f(); };
void g(X x) {
  S s{ x };  // Previously aborted while checking the constraints on the
}            // deduction guide.

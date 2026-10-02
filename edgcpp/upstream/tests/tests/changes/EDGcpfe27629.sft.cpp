//type:fn
//options_all:--c++17 --gn 110400
//remark:[6.7] Assertion failure in make_projection_symbol
// 11/29/24 [EDGcpfe/27629]
//
// Assertion failure in make_projection_symbol
//
// Previously, an ill-formed non-defining class declaration with a base-clause,
// where one of the base classes declares a conversion operator, would trigger an
// assertion failure in make_projection_symbol.
struct B {
  operator bool();
};
struct D : B;  // Previously aborted.  Now an ordinary error.

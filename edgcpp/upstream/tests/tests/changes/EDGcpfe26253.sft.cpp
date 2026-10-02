//type:fp
//options_all:--c++20
//remark:[6.8] Deduced constant template arguments of class type
// 9/5/25   [EDGcpfe/26253,EDGcpfe/27917]
//
// Deduced constant template arguments of class type
//
// The deduction of V produces a value of class type, and that type ought to be
// const-qualified according to the standard (because the underlying model is
// that of a constexpr variable).  However, in this case, the front end failed to
// imbue const-ness on the V during substitution, which caused the requires-
// expression constraint to pass.  As a result, the deleted candidate was
// erroneously preferred in the call g<S{}>().  That is now fixed (the constrained
// candidate now fails the constraint, and thus the second candidate is selected).
// This change also fixes some uses of constexpr variables of class types (which
// sometimes lost their const-qualification when evaluated in constant
// expressions).
struct S {
  int f();  // Not const-qualified
};
template<auto V> int g() requires requires { V.f(); } = delete;
template<auto V> int g();
int r = g<S{}>();

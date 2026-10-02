//type:fp
//options_all:--c++17
//remark:[6.5] Spurious substitution failures when forming implicit deduction guides
// 4/5/23   [EDGcpfe/25306,EDGcpfe/25869]
//
// Spurious substitution failures when forming implicit deduction guides
//
// When forming an implicit deduction guide for the user-declared constructor of
// C, the front end previously instantiated B<T>, causing the static_assert to
// fail.  Additionally, the use of the current instantiation in the second
// template default argument caused substitution to fail.  Both issues are now
// fixed.
template<typename ... T>
struct B {
  static_assert(sizeof ... (T) != 0, "Unexpected");
  static constexpr int v = 0;
};
template<typename ... T>
struct C {
  template<int = B<T ...>::v, typename = C>
  C(T ...);
};
C c{ 1, 2 };  // Previously a deduction failure.  Now okay.

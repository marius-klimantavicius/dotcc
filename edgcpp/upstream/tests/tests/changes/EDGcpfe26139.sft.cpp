//type:fp
//options_all:--c++20
//remark:[6.6] Substitution of member functions
// 10/18/23 [EDGcpfe/26139]
//
// Substitution of member functions
//
// Previously, the front end failed to substitute member functions.
// with --c++20:
template<typename T> struct A {
  static constexpr T f() { return true; }
  static int g() requires (f());
};
int i = A<bool>::g();  // Previously a spurious error.  Now okay.

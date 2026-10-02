//type:fp
//options_all:--ms_c++latest --microsoft_version 1912
//remark:[5.0] Incorrect substitution of C++17 exception specification
// 1/5/18   [EDGcpfe/19031]
//
// Incorrect substitution of C++17 exception specification
//
// In some C++17 cases involving default template arguments, the front end did not
// correctly substitute the exception specification of a member template during
// the deduction process for that member template.  This could result in spurious
// errors later on.
//
// This is now fixed.
template<typename T> struct X {
  template<typename U = T> X() noexcept(__is_nothrow_constructible(U)) {}
};
static_assert(__is_nothrow_constructible(X<int>));
  // Previously, this static_assert failed because the noexcept specifier
  // for the constructor template was not correctly substituted.

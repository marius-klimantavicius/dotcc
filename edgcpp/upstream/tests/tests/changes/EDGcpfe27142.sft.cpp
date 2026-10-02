//type:fp
//options_all:--c++11
//remark:[6.7] Spurious "too many arguments" error on empty pack expansion
// 4/16/24  [EDGcpfe/27142]
//
// Spurious "too many arguments" error on empty pack expansion
//
// When the member declarations for C<0> are instantiated, the template arguments
// for D<0>::X (i.e., <int, T ...>) are matched against the corresponding template
// parameter list (i.e., <typename U>).  Even though T is an empty pack in this
// instantiation, this previously elicited a spurious "too many arguments" error.
// That is now fixed.
template<int> struct D {
  template<typename U> using X = U;
};
template<int I, typename ... T> struct C {
  template<typename>
  using A = typename D<I>::template X<int, T ...>;  // Previously a spurious
};                                                  // error.  Now okay.
C<0> c;

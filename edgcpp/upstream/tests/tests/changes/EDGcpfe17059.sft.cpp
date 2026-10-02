//type:fp
//options_all:--c++11
//remark:[6.5] Substitution failures for nested alias templates
// 1/26/23  [EDGcpfe/17059,EDGcpfe/18006,EDGcpfe/19579,EDGcpfe/20728,
//           EDGcpfe/21691,EDGcpfe/24011,EDGcpfe/25159,EDGcpfe/25240,
//           EDGcpfe/25577]
//
// Substitution failures for nested alias templates
//
// Previously, substitution failures for nested alias templates were not
// considered to be in the immediate context and could therefore result in
// spurious errors.
template<class T> struct B {
  template<class U>
  using C = typename U::type;  // Previously a spurious error.
};
template<class T, class U> typename B<T>::template C<U> f(T, U, int);
template<class T, class U> int f(T, U, long);
int i = f(1, 2, 3);

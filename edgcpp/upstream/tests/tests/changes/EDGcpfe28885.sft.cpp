//type:fp
//options_all:--clang_v 220100
//remark:Clang compatibility: exclude_from_explicit_instantiation attribute
// 6/15/26  [EDGcpfe/28885]
//
// Clang compatibility: exclude_from_explicit_instantiation attribute
//
// The front end now accepts Clang's exclude_from_explicit_instantiation attribute
// when clang_version >= 80000.
template<typename T> struct C;
template<typename T>
struct B {
  __attribute__((exclude_from_explicit_instantiation)) int f() {
    return C<T>::v;
  }
};
template struct B<int>;  // Does not cause the instantiation of B<int>::f().

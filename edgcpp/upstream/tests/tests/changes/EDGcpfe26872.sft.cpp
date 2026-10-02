//type:fp
//options_all:--c++11
//remark:[6.7] Equivalence of dependent alias template specializations
// 4/11/24  [EDGcpfe/26872]
//
// Equivalence of dependent alias template specializations
//
// Previously, the front end did not consider a dependent alias template
// specialization to be equivalent to the aliased type when that type was
// cv-qualified.  Now it is considered equivalent if the alias template is not
// constrained and the aliased type names all template parameters (in other cases,
// the types are still considered to be distinct as substitution into the alias
// template could result in additional substitution failures).
// --c++11:
template<typename> class C {};
template<typename T> using A = const C<T>;
template<typename T> struct B {
  void f(C<A<T>>);
};
template<typename T>
void B<T>::f(C<const C<T>>) {}  // Previously a spurious error.  Now okay.

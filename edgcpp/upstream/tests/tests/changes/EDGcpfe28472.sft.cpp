//type:fp
//options_all:--c++11
//remark:[6.8] Equivalence of type template arguments using typedef names
// 10/9/25  [EDGcpfe/28472]
//
// Equivalence of type template arguments using typedef names
//
// The changes for EDGcpfe/26872 (in version 6.7) introduced a regression where
// type template arguments using typedef names were not always considered
// equivalent.
template<typename> struct B;
template<typename T> using A = B<T>;
template<typename T> struct C {
  using AT = A<T>;
  void f(C<AT> *);
};
template<typename T>
void C<T>::f(C<B<T>> *) {}  // Previously a spurious error.  Now okay.

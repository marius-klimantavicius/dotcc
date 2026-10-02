//type:fp
//options_all:--c++11
//remark:[6.8] Deduction failure for non-type template parameter of dependent pointer type
// 4/11/25  [EDGcpfe/28075]
//
// Deduction failure for non-type template parameter of dependent pointer type
//
// Previously, using a nullptr constant as the template argument for a non-type
// template parameter of dependent pointer type could result in a spurious
// deduction failure.
template<typename> struct X {
  using type = int;
};
template<typename T, typename X<T>::type * = nullptr>
struct C { };
template<typename T> int f(C<T>);
int i = f(C<int>());  // Previously a spurious error.  Now okay.

//type:fp
//options_all:--g++
//remark:[6.7] GCC/Clang compatibility: functionally-equivalent member templates in class
// 4/15/24  [EDGcpfe/25261,EDGcpfe/26579]
//
// GCC/Clang compatibility: functionally-equivalent member templates in class
// template instantiations
//
// The changes made for EDGcpfe/18549 (see entry of 7/11/18) approximated the
// behavior of GCC and Clang by not folding non-dependent sub-expressions after
// encountering a dependent sub-expression.  However, in cases where the
// non-dependent sub-expression preceded any dependent part, this approach did not
// work.  Instead, the front end now doesn't consider dependent template parameter
// or function parameter types to be equivalent when checking for redeclarations
// in class template instantiations.
template<bool>
struct C {};
template<int I>
struct A {
  template<bool B>
  void f(C<I == 1 && B>);
  template<bool B>
  void f(C<I == 2 && B>);  // Now accepted in GNU and Clang modes
};
A<0> a;

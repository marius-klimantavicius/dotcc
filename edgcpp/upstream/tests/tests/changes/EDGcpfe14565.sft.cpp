//type:fp
//options_all:--c++11
//remark:[4.9] constexpr applied to array of dependent type
// 11/8/13  [EDGcpfe/14565]
//
// constexpr applied to array of dependent type
//
// The front end had emitted a spurious error when the constexpr keyword is
// applied to a variable whose type is an array of template-dependent elements.
template <class T> struct A {
  static void f() {
    constexpr T x[] = { 0 };
  }
};
void f() {
  A<int>::f();
}

//type:fp
//options_all:--c++14
//remark:[6.3] Spurious error on alignas in class template
// 11/11/21 [EDGcpfe/24822]
//
// Spurious error on alignas in class template
//
// A spurious error had been given on a member definition that used alignas in a
// class template definition and is now fixed.
template <class T> struct A {
  struct alignas(64) B {} b;
};
A<int> a;

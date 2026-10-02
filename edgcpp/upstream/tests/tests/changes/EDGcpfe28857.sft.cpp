//type:fp
//options_all:--c++17
//remark:CTAD for type template template parameters
// 5/20/26  [EDGcpfe/28857]
//
// CTAD for type template template parameters
//
// The front end now supports class template argument deduction (CTAD) when the
// placeholder for a deduced class type designates a type template template
// parameter, as specified in WG21 paper P3865R3.  Because the paper was adopted
// as a Defect Report, this change applies in all modes that support class
// template argument deduction.
template<class A, class B> struct same { enum { v = 0 }; };
template<class A> struct same<A, A> { enum { v = 1 }; };
template<typename T = char>
struct C {
  using type = T;
  C(const char *);
};
template<template<typename T = int> class TT>
void f() {
  TT t = "";  // Previously deduced as C<char>, now deduced as C<int>
  using D = typename decltype(t)::type;
  static_assert(same<D, int>::v, "");
}
template void f<C>();

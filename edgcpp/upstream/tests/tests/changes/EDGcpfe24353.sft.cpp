//type:fp
//options_all:--c++11
//remark:[6.6] Substitution of alias template containing a function parameter pack
// 7/4/23   [EDGcpfe/24353,EDGcpfe/24386,EDGcpfe/24492,EDGcpfe/24598,
//           EDGcpfe/26367]
//
// Substitution of alias template containing a function parameter pack
//
// Previously, substitution of an alias template containing a function parameter
// pack incorrectly marked the parameter as a pack element.  This could result in
// spurious declaration matching errors.
template<typename ... B> using P = void (*)(B ...);
struct C {
  template<typename ... A>
  void f(P<A ...>);
};
template<typename ... A>
void C::f(void (*)(A ...))  // Previously a spurious error.  Now okay.
{ }

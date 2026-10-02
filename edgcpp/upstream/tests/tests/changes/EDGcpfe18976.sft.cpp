//type:fp
//options_all:--c++11
//remark:[6.7] Matching of template template parameters containing parameter packs
// 1/15/24  [EDGcpfe/18976,EDGcpfe/21888,EDGcpfe/24613,EDGcpfe/26723,
//           EDGcpfe/26774]
//
// Matching of template template parameters containing parameter packs
//
// According to the standard, a template template argument A also matches a
// template template parameter P containing a parameter pack if each of A's
// template parameters matches the corresponding template parameter of P.  The
// front end, however, did not allow this more lenient matching during
// substitution of deduced function template arguments.
// --c++11:
template<typename>
struct A { };
template<template<typename, typename...> class C, typename T, typename... U>
int f(C<T, U...>);
int i = f(A<int>());  // Previously a spurious error.  Now okay.

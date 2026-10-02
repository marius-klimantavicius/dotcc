//type:fp
//options_all:--c++17
//remark:[6.7] Substitution of default template argument in template template argument
// 6/20/24  [EDGcpfe/27273]
//
// Substitution of default template argument in template template argument
//
// Previously, when substituting into the return type of g, substitution of the
// default argument of ns::C was performed in the context of the declaration of W
// instead of the context of ns::C.  This is now fixed.
namespace ns {
  template<typename> struct D {};
  template<typename T, typename = D<T>>  // Previously a spurious error.
  struct C {};                           // Now okay.
}
template<template<typename> typename W, typename T>
W<T> g();
auto v = g<ns::C, int>();

//type:fp
//options_all:--c++11
//remark:[6.6] Equivalence of alias templates
// 8/9/23   [EDGcpfe/20656,EDGcpfe/26160,EDGcpfe/26449]
//
// Equivalence of alias templates
//
// The direction of Core issue 1286 specifies that an alias template is equivalent
// to the aliased template if the template parameter lists (including default
// arguments) are equivalent and the template argument list consists of a list of
// identifiers naming each template parameter in the order they appear in the
// template parameter list.
template<typename> class C;
template<typename T> using A = C<T>;
template<template<typename> class>
class B { };
B<A> b = B<C>{};  // Previously a spurious error.  Now okay.

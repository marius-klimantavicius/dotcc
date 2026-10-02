//type:fp
//options_all:--c++17
//remark:[6.7] Matching of template template parameters with auto-type non-type parameters
// 12/20/23 [EDGcpfe/26764]
//
// Matching of template template parameters with auto-type non-type parameters
//
// According to the rules in [temp.arg.template], this is currently ill-formed as
// the template parameter is not at least as specialized as the template argument.
// Most implementations, however, accept this, and it is anticipated that the C++
// committee will eventually relax that rule for auto-type non-type template
// parameters in a similar way to unconstrained template template parameters (see
// committee paper P1616R1).  The front end has therefore been changed to also
// accept this.
template<int> class A;
template<template<auto> class P> int f();
int i = f<A>();  // Previously an error. Now okay.

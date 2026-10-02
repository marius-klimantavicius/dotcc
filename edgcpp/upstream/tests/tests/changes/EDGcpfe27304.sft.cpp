//type:fp
//options_all:--c++11
//remark:[6.7] Abort on substitution of dependent non-type template parameter type
// 6/12/24  [EDGcpfe/27304]
//
// Abort on substitution of dependent non-type template parameter type
//
// In some cases involving an alias template, the substitution of explicit
// template arguments could result in an abort due to a null pointer indirection
// in get_template_arg_by_list_pos when substituting the type of a dependent
// non-type template parameter.
template<typename T, T> struct C {};
template<typename T> using A = C<T, 0>;
template<template<typename> class W, typename U> W<U> f(U);
auto v = f<A>(1);  // Previously aborted.  Now okay.

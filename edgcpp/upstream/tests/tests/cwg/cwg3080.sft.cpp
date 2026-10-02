//type:fp
//options:--c++11:--c++26
//options_all:-A -tused

template<template<typename> class TT>
struct B
{ };

template<typename T>
struct C
{ };

template<typename T>
using A = C<T *>;

template<template<typename> class TT>
void f()
{
  B<C> b1;
  B<A> b2;
  B<TT> b3;
}

template void f<C>();

//cwg: 3080
//title: Clarify kinds of permitted template template arguments
//meeting: Kona 11/25
//edg_status: Passes

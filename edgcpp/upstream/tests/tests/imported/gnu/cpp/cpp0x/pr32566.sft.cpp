//type: fn
//options: --c++11
// { dg-do compile { target c++11 } }
template<int...> struct A;

template<template<int> class... T> struct A<T...> {}; // { dg-error "mismatch|expected" }

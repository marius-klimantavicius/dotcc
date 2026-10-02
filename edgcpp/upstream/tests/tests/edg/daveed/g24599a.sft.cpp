//remark:Parameter pack substitutions and default args
//options:--c++17;fp

template<typename, typename = int> struct X {};

template<typename T> T f(T*);
template<typename ... Ts> using F = decltype(f<Ts...>(nullptr));


template<typename... Ts> struct S {
  template<template<typename...> class TT = X,
           F<TT<Ts>...>* = nullptr>
    S() {}
};

S<float> sf;

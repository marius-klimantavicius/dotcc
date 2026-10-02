//type:fp
//options:--c++20:--ms_c++20 --microsoft_version 1952
//options_all:-tused

template<typename...> constexpr bool v = true;

template<bool, class T> struct enable_if {};
template<class T> struct enable_if<true, T> { using type = T; };
template<bool B, class T> using enable_if_t = typename enable_if<B, T>::type;

template<typename T>
T declval();

struct X {};

template<typename T>
struct C
{
  template<class... Ts, enable_if_t<v<T, Ts...>, int> = 0>
  C(X, Ts&&...);

  C(T);
};

template<template<class...> class TT, class U, class... Ts>
auto g()  // assertion failed: copy_pack_expansion_descr_with_substitution
{
  return requires { TT(declval<U>(), declval<Ts>()...); };
}

template<template<class...> class TT, typename U, typename ... Ts,
    class = decltype(g<TT, U, Ts...>())>
int f(U, Ts ...);

int i = f<C>(1);

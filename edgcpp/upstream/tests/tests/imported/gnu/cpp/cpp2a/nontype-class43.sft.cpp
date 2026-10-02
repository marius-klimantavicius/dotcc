//type: fp
//options: --c++20
// PR c++/97134
// { dg-do compile { target c++20 } }

template<typename T>
struct templ {};

template<templ... Vs>
struct wrapper {};

template<templ... Vs> requires true
struct wrapper<Vs...> {};

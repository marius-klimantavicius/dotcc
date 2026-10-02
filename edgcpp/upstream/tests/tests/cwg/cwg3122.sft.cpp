//type:fp
//options: -A --c++26 --set_flag reflection

#include <meta>

template <typename T>
consteval void f() {
  T v;
  constexpr std::meta::info R = std::meta::type_of(^^v);
}

template void f<int>();

//cwg: 3122
//title: Inadequate value-dependence for reflect-expressions
//meeting: Croydon 3/26

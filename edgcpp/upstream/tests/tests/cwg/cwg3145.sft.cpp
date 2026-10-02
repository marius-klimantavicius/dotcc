//type:fp
//options: -A --c++26 --set_flag reflection

#include <meta>

template<int> int x [[=1]];

consteval void f() {
  static_assert(std::meta::annotations_of(^^x<0>) != std::meta::annotations_of(^^x<1>));
}

//cwg: 3145
//title: Uniqueness of annotations
//meeting: Croydon 3/26

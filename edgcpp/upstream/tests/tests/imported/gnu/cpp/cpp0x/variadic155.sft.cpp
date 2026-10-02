//type: fn
//options: --c++11
// { dg-do compile { target c++11 } }

template <typename T> struct A {};
template <int... I> struct B: A<I...> {}; // { dg-error "type" }

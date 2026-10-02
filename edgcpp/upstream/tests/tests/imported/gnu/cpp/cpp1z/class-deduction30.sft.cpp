//type: fp
//options: --c++17
// { dg-do compile { target c++17 } }

template <class T = void> struct A { };

A a{};
A a2;

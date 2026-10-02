//type: fp
//options: --c++11
// PR c++/46245
// { dg-do compile { target c++11 } }

template<auto f()->int> struct A { };

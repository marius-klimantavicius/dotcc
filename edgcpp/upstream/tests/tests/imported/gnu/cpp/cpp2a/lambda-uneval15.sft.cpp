//type: fp
//options: --c++20
// PR c++/100091
// { dg-do compile { target c++20 } }

template<typename = decltype([]{})>
void f() {}

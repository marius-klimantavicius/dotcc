//type: fp
//options: --c++17
// { dg-do compile { target c++17_down } }
// { dg-options "-Wno-pointer-arith" }

struct X {};
bool operator<= (X, X);
template<bool (X, X)> struct Y {};
Y<&operator<=> y;
bool foo (bool (*fn) (X, X), int n) { return n+&operator<=> fn; }

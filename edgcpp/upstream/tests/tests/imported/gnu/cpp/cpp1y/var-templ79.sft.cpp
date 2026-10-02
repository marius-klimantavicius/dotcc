//type: fn
//options: --c++14
// PR c++/109300
// { dg-do compile { target c++14 } }

template<class>
auto x = x<int>; // { dg-error "" }

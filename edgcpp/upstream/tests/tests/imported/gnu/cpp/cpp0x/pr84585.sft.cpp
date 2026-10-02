//type: fn
//options: --c++11
// { dg-do compile { target c++11 } }

template <typename a> void b() {[](auto = template <;{a c(auto){}}}  // { dg-error "" }

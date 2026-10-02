//type: fn
//options: --c++11
// { dg-do compile { target c++11 } }
template<typename... Values, typename T> // { dg-error "end" }
struct backward_tuple {};

//type: fp
//options: --c++11
// { dg-do compile { target c++11 } }

template<decltype(nullptr)>
struct nt{};

nt<nullptr> x;

//type:fn
//options_all:--c++17 -tused -A

template <typename T> auto g(T t) { return t; } // #1
void h() { return g(42); } // error, ambiguous

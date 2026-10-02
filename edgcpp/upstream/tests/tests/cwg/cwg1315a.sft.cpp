//type:fn
//options_all:--c++17 -tused -A
//
template <class T, T t> struct C {};
template <class T> struct C<T, 1>; // error

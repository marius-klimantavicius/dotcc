//type:fn
//options_all:--c++20 -tused -A
template<typename T> extern typename T::get_type x;
template<typename T> auto x = T::get();

template<typename T> extern int y;
template<typename T> auto y = 0;

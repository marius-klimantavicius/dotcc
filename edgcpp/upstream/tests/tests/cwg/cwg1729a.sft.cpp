//type:fn
//options_all:--c++20 -tused -A
template<typename T> struct foo;
template<typename T> struct bar;
template<typename T> extern typename foo<T>::type v;
template<typename T> typename bar<T>::type v;

//cwg: 1729
//title: Matching declarations and definitions of variable templates
//meeting: Virtual 11/20*
//edg_status: Passes

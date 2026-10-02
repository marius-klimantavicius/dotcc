//type:fp
//options_all:--c++20 -tused -A
template<typename T> using pointer = T*;
template<typename U> using pointer = U*;

//cwg: 1896
//title: Repeated alias templates
//meeting: Virtual 11/20*
//edg_status: Passes

//type:fn
//options_all:--c++20 -tused -A
  template <typename> struct A;
  template <unsigned> struct A;

//cwg: 2062
//title: Class template redeclaration requirements
//meeting: Virtual 11/20*
//edg_status: Passes

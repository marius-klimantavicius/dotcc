//type:fn
//options_all:--c++17 -tused -A
  template<typename...> using void_t = void;
  template<typename T> void_t<typename T::foo> f();
  f<int>(); // error, int does not have a nested type foo

//cwg: 1558
//title: Unused arguments in alias template specializations
//meeting: Urbana-Champaign 11/14
//edg_status: Passes

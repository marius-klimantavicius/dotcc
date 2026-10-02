//type:fn
//options_all:--c++17 -tused -A
  template<typename T> using X = T;
  void f(auto (*)()); 

//cwg: 1892
//title: Use of auto in function type
//meeting: Urbana-Champaign 11/14
//edg_status: Passes

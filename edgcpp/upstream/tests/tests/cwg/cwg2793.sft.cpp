//type:fn
//options_all:--c++23 -tused

  void f(int i) { extern int i; } 

//cwg: 2793
//title: Block-scope declaration conflicting with parameter name
//meeting: Kona 11/23
//edg_status: Passes

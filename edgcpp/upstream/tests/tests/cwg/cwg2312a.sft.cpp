//type:fp
//options_all:--c++20 -tused
struct A { mutable int n; }; 
  void f() { 
    const auto [a] = A(); 
    a = 0; 
  } 

//cwg: 2312
//title: Structured bindings and mutable
//meeting: Virtual 11/20
//edg_status: Passes

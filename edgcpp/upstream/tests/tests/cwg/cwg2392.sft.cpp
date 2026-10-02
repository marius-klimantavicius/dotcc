//type:fp
//options_all:--c++20 -A
  template<class T = void> constexpr int f() { T t; return 1; }
  using _ = decltype(new int[f()]);

//cwg: 2392
//title: new-expression size check and constant evaluation
//meeting: Kona 11/22
//edg_status: EDGcpfe/25793

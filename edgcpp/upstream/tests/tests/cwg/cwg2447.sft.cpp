//type:fn
//options_all:--c++20 -tused -A
  template <typename> struct A;
  void f(A<auto> x);
  void g(auto f() -> int);
  A<auto> *ap = static_cast<A<int> *>(0);a // should issue an error here

//cwg: 2447
//title: Unintended description of abbreviated function templates
//meeting: Prague 02/20
//edg_status: EDGcpfe/22366

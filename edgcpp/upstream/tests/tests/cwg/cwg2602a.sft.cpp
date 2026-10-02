//type:fn
//options_all:--c++20 -tused
  template <typename Ty>
  struct A {
    Ty n;
    consteval A() {}
  };


  A<int> a;

//cwg: 2602
//title: consteval defaulted functions
//meeting: Issaquah 2/23
//edg_status: EDGcpfe/26057

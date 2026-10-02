//type:fp
//options_all:--c++17 -tused -A
  struct A;
  extern A a;
  struct A {
    const A& a1 { A{a,a} };   // OK
  };
  A a{a,a};                   // OK

//cwg: 1696
//title: Temporary lifetime and non-static data member initializers
//meeting: Urbana-Champaign 11/14
//edg_status: EDGcpfe/16851

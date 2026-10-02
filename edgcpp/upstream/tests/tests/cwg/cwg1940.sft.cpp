//type:fp
//options_all:--c++17 -tused -A
 struct A {
    union {
      static_assert(true, "");
    };
 };

//cwg: 1940
//title: static_assert in anonymous unions
//meeting: Urbana-Champaign 11/14
//edg_status: EDGcpfe/22081

//type:fn
//options_all:--c++17 -tused -A
//
  struct A {
    struct B {
      union {
        int x = 37;
      };
      union {
        int y = x + 47;  //ill-formed
      };
    } a;
  };

//cwg: 1621
//title: Member initializers in anonymous unions
//meeting: Belfast 11/19
//edg_status: EDGcpfe/22009

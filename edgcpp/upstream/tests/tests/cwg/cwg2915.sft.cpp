//type:fn
//options_all:--c++23

  struct A {
    void f(this void);
  };

//cwg: 2915
//title: Explicit object parameters of type void
//meeting: Wroclaw 11/24
//edg_status: EDGcpfe/27753

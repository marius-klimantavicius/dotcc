//type:fp
//options_all:--c++20 -A
  struct S {
    int i = 1;
  };
  static constexpr S s;

//cwg: 2366
//title: Can default initialization be constant initialization?
//meeting: Cologne 07/19
//edg_status: EDGcpfe/21577
//fixed_in: 6.5

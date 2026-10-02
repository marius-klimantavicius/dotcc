//type:fn
//options_all:--c++17 -tused -A
//
  union U { int a; mutable int b; };
  constexpr U u1 = {1};
  int k = (u1.b = 2);
  constexpr U u2 = u1;

//cwg: 2004
//title: Unions with mutable members in constant expressions
//meeting: Kona 10/15
//edg_status: EDGcpfe/22201

//type:fn
//options_all:--c++17 -tused -A
  struct X { int : 32; int n; } x;
  static_assert((void*)&x == (void*)&x.n);

//cwg: 2254
//title: Standard-layout classes and bit-fields
//meeting: Rapperswil 6/18
//edg_status: Passes

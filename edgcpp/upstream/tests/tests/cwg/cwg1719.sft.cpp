//type:fp
//options_all:--c++17 -tused -A
//I am not sure you really can test this so I am just making sure it compiles
  struct A { int a; char b; };
  struct B { const int b1; volatile char b2; };
  struct C { int c; unsigned : 0; char b; };
  struct D { int d; char b : 4; };
  struct E { unsigned int e; char b; };

//cwg: 1719
//title: Layout compatibility and cv-qualification revisited
//meeting: Urbana-Champaign 11/14
//edg_status: Passes

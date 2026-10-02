//type:fp
//options_all:--c++20 -tused -A
  export module M;
  struct S { int n; };
  typedef S S;
  export typedef S S; // OK, does not redeclare an entity

//cwg: 2491
//title: Export of typedef after its first declaration
//meeting: Virtual 10/21
//edg_status: EDGcpfe/24770

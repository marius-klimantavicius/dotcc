//type:fp
//options_all:--c++17 -tused -A
  struct X { X(); };
  struct Y { explicit operator X(); } y;
  X x{y};

//cwg: 1758
//title: Explicit conversion in copy/move list initialization (Resolved by 1467)
//meeting: Urbana-Champaign 11/14
//edg_status: Passes

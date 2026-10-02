//type:fn
//options_all:--c++14 -tused -A
  struct alignas(8) S {};
  struct alignas(1) U {
    S s;
  };   // Error: U specifies an alignment that is less strict than
       // if the alignas(1) were omitted.

//cwg: 1615
//title: Alignment of types, variables, and members
//meeting: Urbana-Champaign 11/14
//edg_status: Passes

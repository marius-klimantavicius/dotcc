//type:fn
//options_all:--c++ -E
  using module = int;
  module i;       // not a text-line and not a control-line
  int foo() {
    return i;
  }

//cwg: 2927
//title: Unclear status of translation unit with module keyword
//meeting: Wroclaw 11/24
//edg_status: EDGcpfe/27761

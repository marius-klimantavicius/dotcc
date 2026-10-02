//type:fp
//options_all:--c++17 -tused -A
  void f() {
       int i;
       i = {0};
     }

//cwg: 2399
//title: Unclear referent of “expression” in assignment-expression
//meeting: Belfast 11/19
//edg_status: Passes

//type:fn
//options_all:--c++20 -tused -A
  struct A {
    (*operator int*());
  };
  A a;
  int *x = a; 

//cwg: 1726
//title: Declarator operators and conversion function
//meeting: Virtual 2/22
//edg_status: Passes

//type:fn
//options_all:--c++14 -tused -A
  const int&& foo();
  int i;
  decltype(foo()) x1 = i; // type is const int&&

//cwg: 1600
//title: Erroneous reference initialization in example
//meeting: Urbana-Champaign 11/14
//edg_status: Passes

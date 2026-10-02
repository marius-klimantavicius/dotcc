//options_all:--c++23 -A
  struct C { int x; };
  C&& c = { .x = 1 };  // OK

//cwg: 2713
//title: Initialization of reference-to-aggregate from designated initializer list
//meeting: Varna 6/23
//edg_status: Passes

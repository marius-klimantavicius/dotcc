//type:fp
//options_all:--c++17 -tused -A

  struct S {
    S f(S s) { return s; }
  };

//cwg: 2430
//title: Completeness of return and parameter types of member functions
//meeting: Belfast 11/19
//edg_status: Passes

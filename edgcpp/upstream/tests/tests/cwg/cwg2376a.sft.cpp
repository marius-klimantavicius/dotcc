//type:fn
//options_all:--c++20 -tused -A
  template <class ...T> struct A {
    A(T...) {}
  };
  A x[29]{};    // error: no declarator operators allowed

//cwg: 2376
//title: Class template argument deduction with array declarator
//meeting: Cologne 07/19
//edg_status: Passes

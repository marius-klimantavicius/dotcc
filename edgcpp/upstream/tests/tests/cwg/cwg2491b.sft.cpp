//type:fn
//options_all:--c++20 -tused -A
  export module M;
  struct S { int n; };
  export struct S;    // error: exported declaration follows non-exported declaration


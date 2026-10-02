//type:fn
//options_all:--c++17 -tused -A
  struct A {
    A() : v(42) { }  // error
    const int& v;
  };

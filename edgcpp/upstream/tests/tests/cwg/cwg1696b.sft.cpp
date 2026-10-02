//type:fn
//options_all:--c++17 -tused -A
  struct A;
  extern A a;
  struct A {
    const A& a2 { A{} };      // error
  };

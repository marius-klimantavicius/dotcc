//type:fn
//options_all:--c++14 -tused -A
  int f();
  struct S {
    S() : a(f()), b(5) {}
    int a, b;
  };
  const S s;
  constexpr int k = s.b;

//cwg: 1757
//title: Const integral subobjects
//meeting: Urbana-Champaign 11/14
//edg_status: Passes

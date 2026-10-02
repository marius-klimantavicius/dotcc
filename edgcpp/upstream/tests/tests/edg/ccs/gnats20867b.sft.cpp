//type:cp
//options_all:--c++14

struct S {
  S() {
    [&](auto... Is) { f(Is...); };
  }
  void f();
};

S s;

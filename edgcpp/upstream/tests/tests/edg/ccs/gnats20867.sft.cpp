//type:cp
//options_all:--c++14

template<typename Ts>
struct S {
  S() {
    [&](auto... Is) { f(Is...); };
  }
  void f();
};

S<int> s;

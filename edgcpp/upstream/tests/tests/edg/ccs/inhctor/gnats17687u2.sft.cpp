//type:cp
//options::--microsoft_version 1914
//options_all:--c++17

struct A {
  template<typename T> constexpr A(T) {}
  int ai = 10;
};

struct B : A {
  using A::A;
  int bi = 20;
};

struct C : B {
  using B::B;
  int ci = 30;
};

void test() {
  constexpr B b(5);
  constexpr C c(10);
  (void)b;
  (void)c;
}

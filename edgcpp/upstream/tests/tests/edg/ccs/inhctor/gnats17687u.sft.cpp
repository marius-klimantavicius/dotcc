//type:cp
//options::--microsoft_version 1914
//options_all:--c++17

struct A {
  template<typename T> constexpr A(T) {}
};

struct B : A {
  using A::A;
};

struct C : B {
  using B::B;
};

void test() {
  constexpr B b(5);
  constexpr C c(10);
  (void)b;
  (void)c;
}

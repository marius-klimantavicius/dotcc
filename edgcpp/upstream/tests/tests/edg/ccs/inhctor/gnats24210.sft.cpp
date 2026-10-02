//type:cp
//options_all:--c++17

struct A {
  struct B {
    B(int = 0) { }
  };
  struct C : B {
    using B::B;
    C(void *);
  };
};
A::C c;

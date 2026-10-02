//type:fp
//options_all:--c++17 -tused -A
  struct A {
   A() {}
   A(const A &) {}
  };
  struct B {
   operator A() { return A(); }
  } b;
  A a{b};

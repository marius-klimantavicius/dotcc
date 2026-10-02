//type:fn
//options::--g++;cp:--clang;cp:--microsoft;cp
//options_all:--c++11

namespace {
  struct A {
    struct B {
      static void f();
      virtual void g();
    };
  };
  void g() { A::B::f(); }
}

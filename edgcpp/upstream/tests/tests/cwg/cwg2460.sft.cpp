//type:fn
//options_all:--c++20 -tused -A
  template <typename T> struct A { struct B; };

  extern "C" {
  template <typename T>
  struct A<T>::B {
   friend void f(B *) requires true {} // C language linkage applies
  };
  }

  namespace Q {
   extern "C" void f(); // ill-formed redeclaration?
  }

//cwg: 2460
//title: C language linkage and constrained non-template friends
//meeting: Virtual 11/20
//edg_status: Passes

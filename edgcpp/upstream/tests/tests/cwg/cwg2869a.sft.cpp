//options_all:--c++23 -A
struct A {
  static void f() {
    struct B {
      void *g() { return this; }
    };
  }
};

//cwg: 2869
//title: this in local classes
//meeting: St Louis 6/24
//edg_status: Passes

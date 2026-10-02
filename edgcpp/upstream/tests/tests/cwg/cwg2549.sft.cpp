//options_all:--c++20

  struct X{
    X();
    X(X &&) = delete;
    X(const X &);
  };

  void f() {
    X x;
    // Is x an lvalue or an xvalue here?
    void g(int n = (decltype((throw x, 0))()));
  }

  void g() {
    X x;
    struct A {
      void g() {
        try {
          struct Y {
            // Is x an lvalue or an xvalue here?
            void h(int n = (decltype((throw x, 0))()));
          };
        } catch (...) { }
      }
    };
  }

//cwg: 2549
//title: friend declarations and module linkage
//meeting: St Louis 6/24
//edg_status: Passes

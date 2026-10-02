//options_all:--c++23 -tused -A
  struct X {
    X() = default;

    X(const X&) = delete;
    X& operator=(const X&) = delete;

    void f(this X self) { }
  };

  void f() {
    X{}.f();   // OK?
  }

//cwg: 2813
//title: Class member access with prvalues
//meeting: Tokyo 3/24
//edg_status: Passes

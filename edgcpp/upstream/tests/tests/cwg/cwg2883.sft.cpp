//options_all:--c++23 -A
  struct A {
    A() = default;
    A(const A &) = delete;
    constexpr operator int() const { return 42; }
  };
  void f() {
    constexpr A a;
    [=]<typename T, int = a> {};   // OK, not odr-usable from a default template argument, and not odr-used
  }

//cwg: 2883
//title: Definition of "odr-usable" ignores lambda scopes
//meeting: St Louis 6/24
//edg_status: Passes

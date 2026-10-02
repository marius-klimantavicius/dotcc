//options_all_--c++23 -A
  void f() {
    struct B {
      B() {}
      B(const B&) { }
    };
    struct D : B {};

    struct BB { B b; };
    struct DD { D d; };

    true ? BB().b : DD().d; // additional copy in C++03, no copy or move in C++11
  }

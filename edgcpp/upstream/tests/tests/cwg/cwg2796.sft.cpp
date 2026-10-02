//options_all:--c++23 -A
  void f() {}
  void g() noexcept {}

  void q() {
    bool b1 = f == g;     // OK
    bool b2 = f > g;      // error: different types
  }

//cwg: 2796
//title: Function pointer conversions for relational operators
//meeting: Kona 11/23
//edg_status: Passes

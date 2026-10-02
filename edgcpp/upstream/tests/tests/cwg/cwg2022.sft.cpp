//type:fp
//options_all:--c++17 -tused -A
  struct A {
    void *p;
    constexpr A(): p(this) {}
  };

  constexpr A a;        // well-formed, a.p points to a
  constexpr A b = A();  // well-formed, b.p points to b

  void g() {
    A c = A();          // well-formed, c.p may point to c or to an ephemeral temporary
  }

//cwg: 2022
//title: Copy elision in constant expressions
//meeting: Oulu 6/16
//edg_status: Passes

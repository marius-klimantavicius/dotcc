//options_all:--c++20 -tused -A
  struct A { char c; int x; };
  union U { A a; };
  constexpr int f() {
    U u;
    u.a.c = 1;
    u.a.x = 2;
    U v = u; // indeterminate padding bytes read!
    return u.a.x;
  }
  extern constexpr int x = f();

//cwg: 2658
//title: Trivial copying of unions in core constant expressions
//meeting: Issaquah 2/23
//edg_status: Passes

//type:fn
//options_all:--c++20 -tused -A

namespace A {
  int x;
  int f(int);
  int g;
  void h();
}

namespace B {
  int i;
  struct g { };
  struct x { };
  void f(int);
  void f(double);
  void g(char);     // OK: hides struct g
}

void func() {
  int i;
  using B::i;       // error: i declared twiceconflicts
  void f(char);
  using B::f;       // OK: each f is a function
  using A::f;       // OK, but interferes with B::f(int)
  f(1);             // error: ambiguous
  static_cast<int(*)(int)>(f)(1);  // OK: calls A::f
  f(3.5);           // calls B::f(double)
  using B::g;
  g('a');           // calls B::g(char)
  struct g g1;      // g1 has class type B::g
  using A::g;       // error: conflicts with B::g
  void h();
  using A::h;       // error: conflicts
  using B::x;
  using A::x;       // OK: hides struct B::x
  x = 99;           // assigns to A::x
  struct x x1;      // x1 has class type B::x
}

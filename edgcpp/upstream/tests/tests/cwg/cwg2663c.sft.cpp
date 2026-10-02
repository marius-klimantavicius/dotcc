//options_all:--c++23 -A
namespace A {
int x;
}
namespace B {
int i;
struct g { };
struct x { };
void f(int);
void f(double);
void g(char); // OK: hides struct g
  }
  void func() {
int i;
void f(char);
using B::f; // OK: each f is a function
f(3.5); // calls B::f(double)
using B::g;
struct g g1; // g1 has class type B::g
using B::x;
  using A::x;              // OK, hides struct B::x
  using A::x;              // OK, does not conflict with previous using A::x
  x = 99;                  // assigns to A::x
  struct x x1;             // x1 has class type B::x
  }

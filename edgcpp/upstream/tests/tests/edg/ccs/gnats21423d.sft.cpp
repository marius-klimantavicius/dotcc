//type:fn
//options_all:--c++14

struct A {
  constexpr A(bool b) noexcept(false) { if (b) throw 0; }
  constexpr operator unsigned() { return 1; }
  ~A();
};
using T = A[1];
void f()
{
  int a [ T{{true}}[0] ];  // Previously caused abort
}

//options_all:-r -x -tused
//options: --strict;rp

struct A {
  void f();
};
A a;
int main() {
  A a;
  a.f();
}
inline void A::f() { }


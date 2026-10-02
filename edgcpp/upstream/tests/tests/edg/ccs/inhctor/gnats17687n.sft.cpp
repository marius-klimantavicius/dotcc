//type:rp
//options::-DNEG;fn
//options_all:--c++17

extern "C" int printf(const char*, ...);

struct AX {};
struct BX {};
struct CX {};

struct A {
  A() = default;
  A(const A&) { printf("A::A(const A&) called\n"); }
  A(const AX&) { printf("A::A(const AX&) called\n"); }
  int ai = 10;
};

struct B : A {
  B() = default;
  B(const B&) { printf("B::B(const B&) called\n"); }
  B(const BX&) { printf("B::B(const BX&) called\n"); }
  using A::A;
  int bi = 20;
};

struct C : B {
  C() = default;
  C(const C&) { printf("C::C(const C&) called\n"); }
  C(const CX&) { printf("C::C(const CX&) called\n"); }
  using B::B;
  int ci = 30;
};

A a;
#if NEG
B b(a);
C c(b);
#endif
C c2(C());

AX ax;
BX bx;
CX cx;

B bxc(ax);
C cxc(bx);
C cxc2(cx);

int main() {}

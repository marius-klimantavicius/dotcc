//type:rp
//options::-DNEG;fn
//options_all:--c++17

extern "C" int printf(const char*, ...);

struct AX {};
struct BX {};
struct CX {};

struct A {
  A() = default;
  A(A&&) { printf("A::A(A&&) called\n"); }
  A(AX&&) { printf("A::A(AX&&) called\n"); }
  int ai = 10;
};

struct B : A {
  B() = default;
  B(B&&) { printf("B::B(B&&) called\n"); }
  B(BX&&) { printf("B::B(BX&&) called\n"); }
  using A::A;
  int bi = 20;
};

struct C : B {
  C() = default;
  C(C&&) { printf("C::C(C&&) called\n"); }
  C(CX&&) { printf("C::C(CX&&) called\n"); }
  using B::B;
  int ci = 30;
};

#if NEG
B b((A&&)A());
C c((B&&)B());
#endif
C c2((C&&)C());

B bxc((AX&&)AX());
C cxc((BX&&)BX());
C cxc2((CX&&)CX());

int main() {}

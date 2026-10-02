//type:rp
//options_all:--c++17

extern "C" int printf(const char*, ...);

struct A {
  A() { printf("A::A() called\n"); }
  A(int x) : ai(x) { printf("A::A(int) called\n"); }
  int ai = 10;
};

struct B : virtual A {
  using A::A;
  B() : bi(25) { printf("B::B() called\n"); }
  int bi = 20;
};

struct C : B, virtual A {
  using A::A;
  using B::B;
  C() : ci(35) { printf("C::C() called\n"); }
  int ci = 30;
};

C c1(5);
C c2;

int main() {
  printf("c1.ai: %d, c1.bi: %d, c1.ci: %d\n", c1.ai, c1.bi, c1.ci);
  printf("c2.ai: %d, c2.bi: %d, c2.ci: %d\n", c2.ai, c2.bi, c2.ci);

  if (c1.ai != 5 || c1.bi != 20 || c1.ci != 30) return 1;
  if (c2.ai != 10 || c2.bi != 25 || c2.ci != 35) return 2;
}

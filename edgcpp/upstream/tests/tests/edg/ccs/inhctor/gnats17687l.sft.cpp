//type:rp
//options::-DNEG;fn
//options_all:--c++17

extern "C" int printf(const char*, ...);

struct A {
  A() = default;
  A(int x) : ai(x) {}
  int ai = 10;
};

struct B {
  B() = default;
  B(int x) : bi(x) {}
  int bi = 20;
};

struct C : virtual A, virtual B {
  using A::A;
  using B::B;
  int ci = 30;
};

C c;
#ifdef NEG
C c2(5);  // Ambiguous between A::A(int) and B::B(int)
#endif

int main() {
  printf("%d %d %d\n", c.ai, c.bi, c.ci);

  if (c.ai != 10 ||
      c.bi != 20 ||
      c.ci != 30) {
    return 1;
  }
}

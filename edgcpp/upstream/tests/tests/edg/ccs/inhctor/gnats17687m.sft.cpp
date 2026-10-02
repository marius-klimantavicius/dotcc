//type:rp
//options::-DNEG;fn
//options_all:--c++17

extern "C" int printf(const char*, ...);

struct A {
  A(int x = 5) : ai(x) {}
  int ai = 10;
};

struct B {
  B(int x = 15, int y = 17) : bi(x) {}
  int bi = 20;
};

struct C : virtual A, virtual B {
  using A::A;
  using B::B;
  int ci = 30;
};

C c;
#ifdef NEG
C c2(2);  // Ambiguous between A::A(int) and B::B(int, int)
#endif
C c3(2, 7);

int main() {
  printf("%d %d %d\n", c.ai, c.bi, c.ci);
  printf("%d %d %d\n", c3.ai, c3.bi, c3.ci);

  if (c.ai != 5 ||
      c.bi != 15 ||
      c.ci != 30) {
    return 1;
  }

  if (c3.ai != 5 ||
      c3.bi != 2 ||
      c3.ci != 30) {
    return 1;
  }
}

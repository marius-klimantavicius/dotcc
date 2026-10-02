//type:rp
//options_all:--c++17

extern "C" int printf(const char*, ...);

struct A {
  A(int x) : ai(x) {}
  int ai = 10;
};

struct B : A {
  using A::A;
  int bi = 20;
};

struct C {
  C() : b(5) {}
  int ci = 30;
  B b;
};

C c;

int main() {
  printf("%d %d %d\n", c.b.ai, c.b.bi, c.ci);

  if (c.b.ai != 5 ||
      c.b.bi != 20 ||
      c.ci != 30) {
    return 1;
  }
}

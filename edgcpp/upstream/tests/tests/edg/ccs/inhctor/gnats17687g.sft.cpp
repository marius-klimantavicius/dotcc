//type:rp
//options_all:--c++17

extern "C" int printf(const char*, ...);

struct A {
  A(int x) : ai(x) {}
  A() : ai(15) {}
  int ai = 10;
};

struct B : public A {
  using A::A;
#ifndef BUG
  B() : bi(25) {}
#endif
  int bi = 20;
};

struct C : public B {
  using B::B;
#ifndef BUG
  C() : ci(35) {}
#endif
  int ci = 30;
};

C c(5);

int main() {
  printf("%d %d %d\n", c.ai, c.bi, c.ci);

  if (c.ai != 5 ||
      c.bi != 20 ||
      c.ci != 30) {
    return 1;
  }
}

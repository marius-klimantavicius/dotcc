//type:fn
//options_all:--c++17

extern "C" int printf(const char*, ...);

struct A {
  A(int x) : ai(x) {}
  int ai = 10;
};

struct VB : virtual A {
  using A::A;
  int vbi = 20;
};

struct B : A {
  using A::A;
  int bi = 30;
};

struct C : virtual B, virtual VB {
  using B::B;
  using VB::VB;
  int ci = 40;
};

C c(5);

int main() {
  printf("%d %d %d\n", c.vbi, c.bi, c.ci);

  if (c.vbi != 20 ||
      c.bi != 30 ||
      c.ci != 40) {
    return 1;
  }
}

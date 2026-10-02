//type:rp
//options_all:--c++17

extern "C" int printf(const char*, ...);

struct A {
  A() : ai(15) {}
  A(int x) : ai(x) {}
  int ai = 10;
};
struct B : A {
  using A::A;
  B() : bi(25) {}
  int bi = 20;
};

struct V1 : virtual B {
  using B::B;
  V1() : v1i(65) {}
  int v1i = 60;
};
struct V2 : virtual B {
  using B::B;
  V2() : v2i(75) {}
  int v2i = 70;
};

struct D : V1, V2 {
  using V1::V1;
  using V2::V2;
  D() : di(85) {}
  int di = 80;
};

struct E : D {
  using D::D;
  E() : ei(95) {}
  int ei = 90;
};

E e(5); // OK: initializes virtual B base class, which initializes the A base class
       // then initializes the V1 and V2 base classes as if by a defaulted default constructor

int main() {
  printf("%d %d %d %d %d %d\n", e.ai, e.bi, e.v1i, e.v2i, e.di, e.ei);

  if (e.ai != 5 ||
      e.bi != 20 ||
      e.v1i != 60 ||
      e.v2i != 70 ||
      e.di != 80 ||
      e.ei != 90) {
    return 1;
  }
}

//type:rp
//options::-DNEG;fn
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

struct C1 : B {
  using B::B;
  C1() : c1i(35) {}
  int c1i = 30;
};
struct C2 : B {
  using B::B;
  C2() : c2i(45) {}
  int c2i = 40;
};

struct D1 : C1, C2 {
  using C1::C1;
  using C2::C2;
  D1() : d1i(55) {}
  int d1i = 50;
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

struct D2 : V1, V2 {
  using V1::V1;
  using V2::V2;
  D2() : d2i(85) {}
  int d2i = 80;
};

struct D3 : V1, C1 {
  using V1::V1;
  using C1::C1;
  D3() : d3i(95) {}
  int d3i = 90;
};

#ifdef NEG
D1 d1(0); // ill-formed: ambiguous
#endif
D2 d2(5); // OK: initializes virtual B base class, which initializes the A base class
          // then initializes the V1 and V2 base classes as if by a defaulted default constructor
#ifdef NEG
D3 d3(7); // ill-formed: ambiguous
#endif

struct M {
  M() : mi(95) {}
  M(int x) : mi(x) {}
  int mi = 90;
};

struct N : M {
  using M::M;
  N() : ni(105) {}
  int ni = 100;
};

struct O : M {
  O() : oi(115) {}
  int oi = 110;
};

struct P : N, O {
};

P p{5}; // OK: use M(5) to initialize N's base class,
        // use M() to initialize O's base class

int main() {
  N *n = &p;
  O *o = &p;
  
  printf("%d %d %d %d %d\n", d2.ai, d2.bi, d2.v1i, d2.v2i, d2.d2i);
  printf("%d %d %d %d\n", n->mi, o->mi, p.ni, p.oi);

  if (d2.ai != 5 ||
      d2.bi != 20 ||
      d2.v1i != 60 ||
      d2.v2i != 70 ||
      d2.d2i != 80) {
    return 1;
  }

  if (n->mi != 5 ||
      o->mi != 95 ||
      p.ni != 100 ||
      p.oi != 115) {
    return 2;
  }
}

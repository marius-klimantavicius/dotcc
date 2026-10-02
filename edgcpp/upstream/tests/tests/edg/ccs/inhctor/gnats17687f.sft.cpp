//type:rp
//options_all:--c++17

extern "C" int printf(const char*, ...);

struct A {
  A() = default;
  A(int x) : ai(x) {}
  int ai = 10;
};

struct B : virtual A {
  using A::A;
  B() : bi(25) {}
  const int bi = 20;
};

struct C : public A {
  C() : ci(35) {}
  int ci = 30;
};

struct D : public B, public C {
  using B::B;
  D() = default;
  int di = 40;
};

int main() {
  D d2(15);
  C* c = &d2;
  B* b = &d2;

  printf("%d %d %d %d %d\n",
	 b->ai, c->ai, d2.bi, d2.ci, d2.di);

  if (b->ai != 15 ||
      c->ai != 10 ||
      d2.bi != 20 ||
      d2.ci != 35 ||
      d2.di != 40) {
    return 1;
  }
}

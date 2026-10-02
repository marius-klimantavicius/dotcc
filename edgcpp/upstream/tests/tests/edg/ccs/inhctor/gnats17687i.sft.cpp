//type:rp
//options_all:--c++17

extern "C" int printf(const char*, ...);

struct A {
  int ai = 10;
  A() {}
};

struct B {
  B() {}
  B(int x) : bi(x) {}
  int bi = 20;
};

struct D : public A, public B {
  int di = 40;
  using B::B;
  D() = default;
};

int main() {
  D d1;
  D d2(15);

  printf("%d %d %d\n",
	 d1.ai, d1.bi, d1.di);

  printf("%d %d %d\n",
	 d2.ai, d2.bi, d2.di);

  if (d1.ai != 10 ||
      d1.bi != 20 ||
      d1.di != 40) {
    return 1;
  }

  if (d2.ai != 10 ||
      d2.bi != 15 ||
      d2.di != 40) {
    return 2;
  }
}

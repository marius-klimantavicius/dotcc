//type:rp
//options::-DNEG;fn:-DNEG2;fn
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
#ifdef NEG
  const
#endif
  int bi;
};

struct C {
#ifdef NEG2
  C() = delete;
#else
  C() : ci(35) {}
#endif
  int ci = 30;
};

struct D : public B, public C {
  using B::B;
  int di = 40;
};

int main() {
  D d2(15);

  printf("%d <ind> %d %d\n",
	 d2.ai, d2.ci, d2.di);

  if (d2.ai != 15 ||
      d2.bi == 25 ||
      d2.ci != 35 ||
      d2.di != 40) {
    return 1;
  }
}

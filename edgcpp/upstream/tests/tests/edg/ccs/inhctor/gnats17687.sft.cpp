//type:rp
//options::-DNEG;fn
//options_all:--c++17

extern "C" int printf(const char*, ...);

struct B1 {
  B1() = default;
  B1(int) : b1i(15) {}

  int b1i = 10;
};

struct B2 {
  B2() = default;
  B2(int) : b2i(25) {}

  int b2i = 20;
};

struct D1 : B1, B2 {
  using B1::B1;
  using B2::B2;

  int d1i = 30;
};
#ifdef NEG
D1 d1(0);    // ill-formed: ambiguous
#endif

struct D2 : B1, B2 {
  using B1::B1;
  using B2::B2;
  D2(int) : d2i(45) {}   // OK: D2::D2(int) hides B1::B1(int) and B2::B2(int)

  int d2i = 40;
};

D2 d2(0);    // calls D2::D2(int)

int main() {
  printf("d2.b1i = %d, d2.b2i = %d, d2.d2i = %d\n", d2.b1i, d2.b2i, d2.d2i);

  if (d2.b1i != 10 ||
      d2.b2i != 20 ||
      d2.d2i != 45) {
    return 1;
  }
}

//type:rp
//options_all:--c++17

extern "C" int printf(const char*, ...);

struct A {
  constexpr A(int x, float f = 1.0f) : ai(x), af(f) {}
  int ai = 10;
  float af;
};

struct B : A {
  using A::A;
  int bi = 20;
  float bf;
};

B b(5);
B b2(15, 3.0f);

int main() {
  printf("%d %0.2f %d <ind>\n", b.ai, b.af, b.bi);
  printf("%d %0.2f %d <ind>\n", b2.ai, b2.af, b2.bi);

  if (b.ai != 5 ||
      b.af != 1.0f ||
      b.bi != 20 ||
      b.bf == 1.0f) {
    return 1;
  }

  if (b2.ai != 15 ||
      b2.af != 3.0f ||
      b2.bi != 20 ||
      b2.bf == 3.0f) {
    return 1;
  }
}

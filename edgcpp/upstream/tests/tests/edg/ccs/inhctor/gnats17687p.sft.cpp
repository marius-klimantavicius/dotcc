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

int main() {
  printf("%d %d %d %d\n", B(5).ai, B(5).bi, B(15).ai, B(15).bi);

  if (B(5).ai != 5 ||
      B(5).bi != 20 ||
      B(15).ai != 15 ||
      B(15).bi != 20) {
    return 1;
  }
}

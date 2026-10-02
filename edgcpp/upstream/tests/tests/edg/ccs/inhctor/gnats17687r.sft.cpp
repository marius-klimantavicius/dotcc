//type:rp
//options_all:--c++17

extern "C" int printf(const char*, ...);

struct A {
  A(...) : ai(5) { printf("Calling A::A(...)\n"); }
  int ai = 10;
};

struct B : A {
  using A::A;
  int bi = 20;
};

B b(5);
B b2(15);

int main() {
  printf("%d %d %d %d\n", b.ai, b.bi, b2.ai, b2.bi);

  if (b.ai != 5 ||
      b.bi != 20 ||
      b2.ai != 5 ||
      b2.bi != 20) {
    return 1;
  }
}

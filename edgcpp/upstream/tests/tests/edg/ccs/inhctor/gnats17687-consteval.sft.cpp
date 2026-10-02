//type:rp
//options::-DNEG;fn
//options_all:--c++20

extern "C" int printf(const char*, ...);

struct A {
  consteval A(int x) : ai(x) {}
  int ai = 10;
};

struct B : A {
  using A::A;
  int bi = 20;
};

consteval B test(int x) {
  return x;
}

int main(int argc, char** argv) {
  B b = test(5);
#if NEG
  B b2 = test(argc);
#endif

  printf("%d %d\n", b.ai, b.bi);

  if (b.ai != 5 ||
      b.bi != 20) {
    return 1;
  }
}

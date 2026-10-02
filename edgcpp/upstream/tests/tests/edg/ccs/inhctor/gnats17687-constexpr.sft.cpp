//type:rp
//options::-DNEG;fn
//options_all:--c++17

extern "C" int printf(const char*, ...);

struct A {
  constexpr A(int x) : ai(x) {}
  int ai = 10;
};

struct B : A {
  using A::A;
  int bi = 20;
};

constexpr B test(int x) {
  return x;
}

int main(int argc, char** argv) {
  constexpr B b = test(5);
#if NEG
  constexpr
#endif
  B b2 = test(argc);

  printf("%d %d %d %d\n", b.ai, b.bi, b2.ai, b2.bi);

  if (b.ai != 5 ||
      b.bi != 20 ||
      b2.ai != 1 ||
      b2.bi != 20) {
    return 1;
  }  
}

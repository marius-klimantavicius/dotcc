//type:rp
//options::--g++:--clang
//options_all:--c++17

extern "C" int printf(const char*, ...);

template <class... T> struct S : public T... {
  using T::T...;
};

struct A {
  A() = default;
  A(int x) : ai(x) {}
  int ai = 10;
};

struct B {
  B() = default;
  B(int x, float f) : bi(x), bf(f) {}
  int bi = 20;
  float bf = 2.0f;
};

S<A, B> s1(5), s2(15, 1.5f);

int main() {
  printf("S1: %d %d %0.2f\n", s1.ai, s1.bi, s1.bf);
  printf("S2: %d %d %0.2f\n", s2.ai, s2.bi, s2.bf);

  if (s1.ai != 5 ||
      s1.bi != 20 ||
      s1.bf != 2.0f)
    return 1;

  if (s2.ai != 10 ||
      s2.bi != 15 ||
      s2.bf != 1.5f)
    return 2;
}

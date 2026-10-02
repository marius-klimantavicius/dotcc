//type:rp
//options::--gnu_version 80100
//options_all:--c++17 -tused

extern "C" int printf(const char*, ...);

struct X {} x;

struct A {
  A() {}
  template<typename T> constexpr A(T) : ai(15) {}
  int ai = 10;
};

struct B : public A {
  using A::A;
};

struct C : B {
  constexpr C() : B(x) {}
  C(int) {}
};

constexpr C c;
C c2(10);

int main() {
  printf("%d %d\n", c.ai, c2.ai);

  if (c.ai != 15 ||
      c2.ai != 10)
    return 1;
}

//type:rp
//options::-DNEG;fn
//options_all:--c++20

extern "C" int printf(const char*, ...);

struct A {
  consteval A(int x) : ai(x) {}
  int ai = 10;
};

struct B {
  int bi = 20;
};

struct C : A, B {
  using A::A;
  int ci = 30;
};

consteval C test(int x) {
  return x;
}

int main(int argc, char** argv) {
  C c = test(5);
  C c2(15);
#if NEG
  C c3 = test(argc);
  C c4(argc);
#endif
  
  printf("%d %d %d\n", c.ai, c.bi, c.ci);
  printf("%d %d %d\n", c2.ai, c2.bi, c2.ci);

  if (c.ai != 5 ||
      c.bi != 20 ||
      c.ci != 30) {
    return 1;
  }

  if (c2.ai != 15 ||
      c2.bi != 20 ||
      c2.ci != 30) {
    return 1;
  }  
}

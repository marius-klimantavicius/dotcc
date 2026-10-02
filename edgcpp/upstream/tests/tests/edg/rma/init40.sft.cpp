//options_all:-r -x -tused
//options: --strict;rp

struct A { char s1[4], s2[4]; };
struct B {
  char s1[4];
  A a;
  char s2[4];
};
B b = { "abc", "def", "ghi", "jkl" };
extern "C" int printf(char *, ...);
int main() {
  printf("b:\t%s %s %s %s\n", b.s1, b.a.s1, b.a.s2, b.s2);
}



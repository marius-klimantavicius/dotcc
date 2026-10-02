//options_all:-r -x -tused
//options: --strict;cn:;rp

// Rewrite rule applied to default argument?
#include <stdio.h>
static int a = 23;
class C {
public:
  static int f(int i = a) { return i; }
  static int a;
  static int g(int i = a) { return i; }
  static int h(int);
};
int C::a = 17;
int C::h(int i = a) { return i; }
main() {
  printf("%d %d %d\n", C::f(), C::g(), C::h());
}


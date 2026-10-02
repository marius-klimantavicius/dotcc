//options_all:-r -x -tused
//options: --strict;cn:;cn

class A {
  int i;
  union {
    int a;
    float b;
  };
  int j;
};
main() {
  A x, y;
  x.a = 1;
  y.b = 1.0;
}


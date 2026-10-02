//options_all:-r -x -tused
//options: --strict;cn:;cp

struct A {
  A();
  A(const A&);
  ~A();
};
A f();
main () {
  int i = 1;
  A x;
  switch (i) {
    case 1:
      x = f();
      break;
    case 2:
      if (i) return 0;
  }
}


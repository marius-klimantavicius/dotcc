//options_all:-r -x -tused
//options: --strict;cn:;rp

struct S {
  static int f() {
    enum { e = 3 };
    return e;
  }
};
main() {
  S s;
  int i = s.f();
}


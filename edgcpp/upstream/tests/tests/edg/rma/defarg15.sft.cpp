//options_all:-r -x -tused --diag_warning=949
//options: --strict;cn:;cn

struct A {
  void f(void (*)(int = x), int = y);
  typedef void (*pf)(void (*)(int = x), int = y);
  void (*m)(void (*)(int = x), int = y);
  static void (*s)(void (*)(int = x), int = y);
  static int x, y;
};


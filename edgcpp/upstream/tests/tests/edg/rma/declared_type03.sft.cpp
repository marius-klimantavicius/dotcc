//options_all:-r -x -tused
//options: --strict;cp

void f(void x());
void f(void x()) { }
struct S {
  static void f(void x());
  static void g(void x()) { }
};
void S::f(void x()) { }


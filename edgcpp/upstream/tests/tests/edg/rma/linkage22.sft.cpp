//options_all:-r -x -tused
//options: --strict;cn:;ln

extern "C" {
struct S {
  void f();
  void g();
  void h();
};
  void S::f() { }
}
void S::g() { }
main() {
  S s;
  s.h();
}


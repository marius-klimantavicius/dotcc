//options_all:-r -x -tused
//options: --strict;cn:;rp

template <class T> void f(T*) { }
struct X {
  void f();
};
void X::f() {
  ::f(this);
}
main() {
  X x;
  x.f();
}



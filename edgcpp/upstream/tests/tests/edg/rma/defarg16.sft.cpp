//options_all:-r -x -tused --diag_warning=949
//options: --strict;cn:;cn

extern "C" void printf(char *, ...);
template <class T> struct X {
  // Typedef
  typedef void (*pf)(int = x);
  // Field
  void (*m)(int = x*10);
  // Static data member
  static void (*s)(int = x*100);
  static int x;
};
typedef X<int> A;
int A::x = 0;
void func(int i) {
  printf("%d\n", i);
}
void (*A::s)(int) = &func;
main() {
  // Call func using interface from typedef
  ++A::x;
  A::pf pf = &func;
  pf();
  // Call func using interface from nonstatic data member
  ++A::x;
  A a;
  a.m = &func;
  (*a.m)();
  // Call func using interface from static data member
  ++A::x;
  (*A::s)();
}


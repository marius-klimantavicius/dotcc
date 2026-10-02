//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn

// How does a local function with non-inline functions get handled?
void F() {
  class A {
    public:
      int a,b,c;
      int f(int i) { return i; };
      int ff(int);                // error -- ARM 9.8
  } x;
  x.a = 1;
  x.b = x.f(3);
  x.c = x.ff(0);
}


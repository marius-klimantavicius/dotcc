//options_all:-r -x -tused
//options: --strict;cn

struct X {
  void f() throw(int);
  void f(int) throw(int);
  void f(double) throw(int);
};
void X::f() throw(char) { }
void X::f(int) throw() { }
void X::f(double) { }


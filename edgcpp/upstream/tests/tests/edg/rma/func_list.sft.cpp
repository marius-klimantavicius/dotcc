//options_all:-r -x -tused
//options: --strict;cn:--diag_warn=260;cp

// Cfront disallows lists of constructor declarations, but the ARM mentions
// no such restriction.  It also disallows a list of ordinary member functions
// in certain cases, apparently when no specifiers are present.
class A {
  A(), A(int), A(int,int);
  a(), b(), c();
  int a(int), b(int), c(int);
  static a(int,int), b(int,int), c(int,int);
};


//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:--diag_warning=260;cp

struct S {
  int f2(int i = 0) { return f1(); } // It is okay to swap order of f1,f2
  int f1() { return f2() + f2(1); }
  struct S1 {
    f(S *sp) {
      sp->f1();
      sp->f2(); // C+FE gives error here
      return sp->f2(1);
    }
  };
};


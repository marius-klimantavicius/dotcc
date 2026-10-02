//type:fp
//options_all:--c++17 -tused -A
struct C;
  void no_opt(C*);
  struct C {
    int c;
    C() : c(0) { no_opt(this); }
  };

  const C cobj;

  void no_opt(C* cptr) {
    int i = cobj.c * 100; // value of cobj.c is unspecified
    cptr->c = 1;
  }

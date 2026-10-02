//options_all:-r -x -tused
//options: --strict;cn

/* Local types used in extern declarations. */
struct A { int i; } a;
void f(struct AA { int ii; } p, void (*pf)(struct AAA { int iii; })) {
  struct B { int j; } b;
  extern void g(struct B);
  g(b);
  { struct C { int k; };
    extern struct C v;
    &v;
  }
}


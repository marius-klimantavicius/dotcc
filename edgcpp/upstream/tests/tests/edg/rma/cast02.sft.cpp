//options_all:-r -x -tused
//options: --strict;cn:;cp

// IL lowering of casts to base classes
struct A {int i;};
struct B {int j;};
struct C : public A, public B {int k;};
struct D {int l;};
struct E : public D, virtual public C {int m;};
struct EE : public E {};
struct F {int n;};
struct G : public F, public EE {int o;};
main () {
  A *pa;
  B *pb;
  C *pc;
  D *pd;
  E e, *pe;
  EE *pee;
  F *pf;
  G g, *pg;
  int a;
  pa = pg;
  pb = pg;
  pc = pg;
  pd = pg;
  pe = pg;
  pee = pg;
  pf = pg;
  g.o = 1;
  pg->o = 1;
  a = pg->o;
  g.n = 1;
  pg->n = 1;
  a = pg->n;
  g.m = 1;
  pg->m = 1;
  a = pg->m;
  g.l = 1;
  pg->l = 1;
  a = pg->l;
  g.k = 1;
  pg->k = 1;
  a = pg->k;
  g.j = 1;
  pg->j = 1;
  a = pg->j;
  g.i = 1;
  pg->i = 1;
  a = pg->i;
  /* Casts to derived type. */
  pg = (G *)&e;
  pg = (G *)pe;
}


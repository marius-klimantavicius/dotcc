//options_all:-r -x -tused
//options: --strict;cn:;cp

// IL lowering of casts to base and derived classes
struct A {int i;};
struct B {int j;};
struct C : public A, public B {int k;};
struct D {int l;};
struct E : public D, virtual public C {int m;};
struct EE : public E {};
struct F {int n;};
struct G : public F, public EE {int o; int f();};
int G::f() {
  i = 1;
  return 0;
}
main () {
  G g, *pg;
  E e;
  g.m = 1;
  pg->m = 1;
  g.i = 1;
  pg->i = 1;
  pg = (G *)&e;
}


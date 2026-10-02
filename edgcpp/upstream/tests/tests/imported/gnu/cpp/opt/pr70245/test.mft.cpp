//type: rp
//options:  pr70245-aux.cc
# 0 "./opt/pr70245.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./opt/pr70245.C"







# 1 "./opt/pr70245.h" 1
extern struct A *a, *i;
extern int b, c, e, l;
int *fn1 (char *, int *);
void fn2 ();
void *fn3 (int *);
struct B { char *b; };
typedef void (*F) (A *, B *, unsigned char *, int *);
struct C { int c[16]; };
struct D { int d; };
struct A { D a1; C a2; };
void *fn4 ();
extern F d;
extern B k;
extern void baz (int);
# 9 "./opt/pr70245.C" 2

struct A *a, *i;
int b, c, e, l;
F d;

static A *
foo (B *x, int *y, int *z)
{
  unsigned char *f = (unsigned char *) fn3 (y);
  D *g = (D *) f;
  A *h;
  if (e || a || c || b || g->d)
    return 0;
  h = (A *) fn4 ();
  __builtin_memcpy (h, a, sizeof (A));
  h->a1 = *(D *) f;
  if (d)
    {
      d (h, x, f + g->d, z);
      if (*z)
 fn2 ();
    }
  return h;
}

static A *
bar (B *x, int *y)
{
  int *j = fn1 (x->b, y);
  if (*y > 0)
    return 0;
  i = foo (x, j, y);
  return i;
}

B k;

void
baz (int x)
{
  if (x)
    bar (0, 0);
  bar (&k, &l);
}

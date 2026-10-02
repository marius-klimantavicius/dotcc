//type: fp
//options: 
# 0 "./opt/pr70245-aux.cc"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./opt/pr70245-aux.cc"




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
# 6 "./opt/pr70245-aux.cc" 2

D m;
A n, o;
int p, q;

int *
fn1 (char *x, int *y)
{
  *y = 0;
  return &p;
}

void
fn2 ()
{
  __builtin_abort ();
}

void *
fn3 (int *x)
{
  *x = 0;
  return (void *) &m;
}

void *
fn4 ()
{
  a = &o;
  o.a1.d = 9;
  m.d = sizeof (D);
  __builtin_memcpy (o.a2.c, "abcdefghijklmnop", 16);
  return (void *) &n;
}

void
fn5 (A *x, B *y, unsigned char *z, int *w)
{
  if (x != &n || y != &k || z != (unsigned char *) (&m + 1))
    __builtin_abort ();
  q++;
}

int
main ()
{
  d = fn5;
  baz (0);
  if (q != 1)
    __builtin_abort ();
}

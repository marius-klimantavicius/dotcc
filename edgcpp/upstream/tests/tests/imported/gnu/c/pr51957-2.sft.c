//type: fp
//options: 
# 0 "./pr51957-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr51957-2.c"




# 1 "./pr51957-1.h" 1
union R { int c; union R *p; };
extern union R *w[];
typedef struct { int t; } T;
typedef struct { void *u; } U;
union R *fn1 (void);
void fn2 (int, const char *, union R *);
void fn3 (void);
int fn4 (union R *);
void foo (U *x);
# 6 "./pr51957-2.c" 2

union R *w[10];

union R *
fn1 (void)
{
  return (union R *) 0;
}

void
fn2 (int x, const char *y, union R *z)
{
}

void
fn3 (void)
{
}

int
fn4 (union R *x)
{
  return 0;
}

int
main ()
{
  return 0;
}

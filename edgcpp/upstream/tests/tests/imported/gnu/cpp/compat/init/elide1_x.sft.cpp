//type: fp
//options: 
# 0 "./compat/init/elide1_x.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/init/elide1_x.C"
# 1 "./compat/init/elide1.h" 1
struct A {
  A ();
  A (const A&);
  ~A ();
};
# 2 "./compat/init/elide1_x.C" 2

extern "C" void abort (void);
extern void f (A);
extern int d;

void
elide1_x (void)
{
  int r;
  f (A ()), r = d;

  if (r >= d || !d)
    abort ();
}

//type: fp
//options: 
# 0 "./compat/init/byval1_x.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/init/byval1_x.C"
# 1 "./compat/init/byval1.h" 1
struct C
{
  int m;
  C();
  ~C();
};
# 2 "./compat/init/byval1_x.C" 2

extern "C" void abort (void);
extern void Foo (C c);
extern int r;

void
byval1_x ()
{
  C c;

  Foo (c);
  if (r != 0)
    abort ();
}

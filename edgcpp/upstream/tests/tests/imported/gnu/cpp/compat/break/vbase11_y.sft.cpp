//type: fp
//options: 
# 0 "./compat/break/vbase11_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/break/vbase11_y.C"
extern "C" void abort (void);

# 1 "./compat/break/vbase11.h" 1
struct base
{
  short b;
  virtual int foo();
};

struct derived: virtual base
{
  int d;
  virtual int foo();
  virtual int bar();
};
# 4 "./compat/break/vbase11_y.C" 2

void vbase11_y (derived& d)
{
  if (d.foo() != 2)
    abort ();
  if (d.bar() != 3)
    abort ();
}

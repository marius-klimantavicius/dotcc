//type: fp
//options: 
# 0 "./compat/break/vbase11_x.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/break/vbase11_x.C"
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
# 2 "./compat/break/vbase11_x.C" 2

extern void vbase11_y (derived&);

int base::foo() { return 1; }
int derived::foo() { return 2; }
int derived::bar() { return 3; }

void vbase11_x ()
{
  derived d;

  vbase11_y (d);
}

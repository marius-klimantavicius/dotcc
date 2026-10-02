//type: fp
//options: 
# 0 "./compat/break/vbase11_main.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/break/vbase11_main.C"




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
# 6 "./compat/break/vbase11_main.C" 2

extern void vbase11_x (void);

int
main ()
{
  vbase11_x ();
}

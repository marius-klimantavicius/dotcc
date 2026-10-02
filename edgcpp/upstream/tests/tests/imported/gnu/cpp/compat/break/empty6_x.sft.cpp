//type: fp
//options: 
# 0 "./compat/break/empty6_x.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/break/empty6_x.C"
# 1 "./compat/break/empty6.h" 1
struct A {};

struct B {
  A a;
  virtual void f () {}
  int i;
};
# 2 "./compat/break/empty6_x.C" 2

extern void empty6_y (B&);

void empty6_x ()
{
  B b;

  b.i = 7;

  empty6_y (b);
}

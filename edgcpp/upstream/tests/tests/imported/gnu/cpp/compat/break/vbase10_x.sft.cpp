//type: fp
//options: 
# 0 "./compat/break/vbase10_x.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/break/vbase10_x.C"
# 1 "./compat/break/vbase10.h" 1
struct A {
  virtual void f();
  char c1;
};

struct B {
  B();
  char c2;
};

struct C : public A, public virtual B {
};
# 2 "./compat/break/vbase10_x.C" 2

extern void vbase10_y (C&);

void vbase10_x ()
{
  C c;

  c.c1 = 1;
  c.c2 = 2;

  vbase10_y (c);
}

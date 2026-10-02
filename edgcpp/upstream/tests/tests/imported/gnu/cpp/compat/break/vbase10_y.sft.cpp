//type: fp
//options: 
# 0 "./compat/break/vbase10_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/break/vbase10_y.C"
extern "C" void abort (void);

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
# 4 "./compat/break/vbase10_y.C" 2

void A::f () {}
B::B() {}

void vbase10_y (C& c)
{
  if (c.c1 != 1)
    abort ();
  if (c.c2 != 2)
    abort ();
}

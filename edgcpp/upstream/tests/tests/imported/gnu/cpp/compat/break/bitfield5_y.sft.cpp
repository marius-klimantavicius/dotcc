//type: fp
//options: 
# 0 "./compat/break/bitfield5_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/break/bitfield5_y.C"
extern "C" void abort (void);

# 1 "./compat/break/bitfield5.h" 1
struct A {
  virtual void f();
  int f1 : 1;
};

struct B : public A {
  int f2 : 1;
  int : 0;
  int f3 : 4;
  int f4 : 3;
};
# 4 "./compat/break/bitfield5_y.C" 2

void A::f () {}

void bitfield5_y (B& b)
{
  if (b.f3 != 7)
    abort ();
  if (b.f4 != 3)
    abort ();
}

//type: fp
//options: 
# 0 "./compat/break/empty6_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/break/empty6_y.C"
extern "C" void abort (void);

# 1 "./compat/break/empty6.h" 1
struct A {};

struct B {
  A a;
  virtual void f () {}
  int i;
};
# 4 "./compat/break/empty6_y.C" 2

void empty6_y (B& b)
{
  if (b.i != 7)
    abort ();
}

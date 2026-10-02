//type: fp
//options: 
# 0 "./compat/eh/dtor1_x.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/eh/dtor1_x.C"
extern "C" void exit (int);
extern "C" void abort (void);

# 1 "./compat/eh/dtor1.h" 1
struct A {
  ~A();
};

struct B: public A {
  ~B();
};
# 5 "./compat/eh/dtor1_x.C" 2

int r;

void dtor1_x ()
{
  { B b; }
  if (r != 0)
    abort ();
  exit (0);
}

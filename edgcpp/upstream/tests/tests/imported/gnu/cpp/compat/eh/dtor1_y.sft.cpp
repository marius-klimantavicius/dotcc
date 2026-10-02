//type: fp
//options: 
# 0 "./compat/eh/dtor1_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/eh/dtor1_y.C"
extern int r;
int ad;

# 1 "./compat/eh/dtor1.h" 1
struct A {
  ~A();
};

struct B: public A {
  ~B();
};
# 5 "./compat/eh/dtor1_y.C" 2

A::~A () { ++ad; }

B::~B ()
try
  {
    throw 1;
  }
catch (...)
  {
    if (!ad)
      r = 1;
    return;
  }

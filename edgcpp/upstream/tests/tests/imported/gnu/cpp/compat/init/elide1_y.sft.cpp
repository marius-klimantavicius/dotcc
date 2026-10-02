//type: fp
//options: 
# 0 "./compat/init/elide1_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/init/elide1_y.C"
# 1 "./compat/init/elide1.h" 1
struct A {
  A ();
  A (const A&);
  ~A ();
};
# 2 "./compat/init/elide1_y.C" 2

int d;

A::A () { }
A::A (const A&) { }
A::~A() { ++d; }

void f (A a) { }

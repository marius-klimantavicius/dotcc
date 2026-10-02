//type: fp
//options: 
# 0 "./compat/init/dtor1_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/init/dtor1_y.C"
# 1 "./compat/init/dtor1.h" 1
struct B
{
  int x;
  B (int);
  ~B ();
};

struct C1 : public B {
  C1 (int);
};

struct C2 : public B {
  C2 (int);
};

struct D : public B {
  D (int);
};

struct E : public B {
  E (int);
};

struct A
  : public C1, C2, virtual public D, virtual public E
{
  A ();
  B x1;
  B x2;
};
# 2 "./compat/init/dtor1_y.C" 2

extern "C" void abort ();

int d = 5;

B::B (int i) : x (i) { }
B::~B () { if (d-- != x) abort (); }

C1::C1 (int i) : B (i) {}

C2::C2 (int i) : B (i) {}

D::D (int i) : B (i) {}

E::E (int i) : B (i) {}

A::A () : D (0), E (1), C1 (2), C2 (3), x1(4), x2(5) {}

//type: fp
//options: 
# 0 "./opt/dtor2-aux.cc"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./opt/dtor2-aux.cc"



# 1 "./opt/dtor2.h" 1
struct A
{
  A ();
  ~A ();
};

struct B
{
  A b;
  virtual void mb ();
  B (int);
  virtual ~B ();
};

struct C : public B
{
  virtual void mc ();
  C (int);
  ~C ();
};

inline C::~C () {}

struct D : public C
{
  A d;
  D (int);
  ~D ();
};
# 5 "./opt/dtor2-aux.cc" 2

A::A () {}
A::~A () {}

void B::mb () {}
B::B (int) {}
B::~B () {}

void C::mc () {}
C::C (int x) : B (x) {}

D::~D () {}

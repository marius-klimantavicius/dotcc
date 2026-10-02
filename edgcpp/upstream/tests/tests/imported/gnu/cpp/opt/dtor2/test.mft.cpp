//type: lp
//options:  dtor2-aux.cc
# 0 "./opt/dtor2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./opt/dtor2.C"





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
# 7 "./opt/dtor2.C" 2

D::D (int x) : C (x) {}

int
main ()
{
}

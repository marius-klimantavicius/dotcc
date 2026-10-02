//type: fp
//options: 
# 0 "./compat/eh/nrv1_x.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/eh/nrv1_x.C"
extern "C" void exit (int);
extern "C" void abort (void);

# 1 "./compat/eh/nrv1.h" 1
struct A
{
  A();
  ~A();
};
# 5 "./compat/eh/nrv1_x.C" 2

extern A f (void);

int c, d;

void nrv1_x ()
{
  try
    { A a = f(); }
  catch (...) { }
  if (d < c)
    abort ();
  exit (0);
}

A::A() { ++c; }
A::~A() { ++d; }

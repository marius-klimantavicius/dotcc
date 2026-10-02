//type: fp
//options: 
# 0 "./compat/eh/filter1_x.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/eh/filter1_x.C"
# 1 "./compat/eh/filter1.h" 1
struct a
{
  a();
  ~a();
};
# 2 "./compat/eh/filter1_x.C" 2

extern "C" void exit (int);
extern "C" void abort (void);
extern void ex_test (void);

void
filter1_x ()
{
  try
    {
      ex_test ();
    }
  catch (...)
    {
    }
  abort ();
}

a::a() { }
a::~a() { exit (0); }

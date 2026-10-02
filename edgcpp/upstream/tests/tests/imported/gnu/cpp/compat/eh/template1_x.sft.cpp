//type: fp
//options: 
# 0 "./compat/eh/template1_x.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/eh/template1_x.C"
extern "C" void exit (int);
extern "C" void abort (void);

# 1 "./compat/eh/template1.h" 1
class A {};

template <class T>
struct B
{
  typedef A E;
};

template <class T>
struct C
{
  typedef B<T> D;
  typedef typename D::E E;
  void f()



  ;
};
# 5 "./compat/eh/template1_x.C" 2

void template1_x ()
{
  int caught = 0;
  try
    {
      C<int> x;
      x.f();
    }
  catch (A)
    {
      ++caught;
    }
  if (caught != 1)
    abort ();
  exit (0);
}

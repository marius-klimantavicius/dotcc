//type: fp
//options: 
# 0 "./compat/eh/ctor1_x.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/eh/ctor1_x.C"
extern "C" void abort (void);
extern "C" void exit (int);

# 1 "./compat/eh/ctor1.h" 1
struct Foo
{
  ~Foo ();
};

struct Bar
{
  ~Bar ()



  noexcept(false)

  ;
  Foo f;
};
# 5 "./compat/eh/ctor1_x.C" 2

bool was_f_in_Bar_destroyed=false;

void ctor1_x ()
{
  try
    {
      Bar f;
    }
  catch(int i)
    {
      if(was_f_in_Bar_destroyed)
 {
   exit (0);
 }
    }
  abort ();
}

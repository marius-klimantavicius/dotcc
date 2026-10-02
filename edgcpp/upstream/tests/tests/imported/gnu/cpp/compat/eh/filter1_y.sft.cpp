//type: fp
//options: 
# 0 "./compat/eh/filter1_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/eh/filter1_y.C"
# 1 "./compat/eh/filter1.h" 1
struct a
{
  a();
  ~a();
};
# 2 "./compat/eh/filter1_y.C" 2

struct e1 {};
struct e2 {};

void
ex_test ()
{
  a aa;
  try
    {
      throw e1 ();
    }
  catch (e2 &)
    {
    }
}

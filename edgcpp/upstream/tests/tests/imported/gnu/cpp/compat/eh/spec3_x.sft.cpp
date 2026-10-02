//type: fp
//options: 
# 0 "./compat/eh/spec3_x.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/eh/spec3_x.C"
# 1 "./compat/eh/spec3.h" 1
class Base {};

struct A : virtual public Base
{
  A();
};

struct B {};
# 2 "./compat/eh/spec3_x.C" 2

extern void func ()



;

void spec3_x (void)
{
  try { func(); }
  catch (A& a) { }
}

//type: fp
//options: 
# 0 "./compat/eh/nrv1_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/eh/nrv1_y.C"
# 1 "./compat/eh/nrv1.h" 1
struct A
{
  A();
  ~A();
};
# 2 "./compat/eh/nrv1_y.C" 2

A f()
{
  A nrv;
  throw 42;
  return nrv;
}

//type: fp
//options: --c++11
// PR c++/60432
// { dg-do compile { target c++11 } }

struct A
{
  int a;
  static constexpr int A::*p = &A::a;
};

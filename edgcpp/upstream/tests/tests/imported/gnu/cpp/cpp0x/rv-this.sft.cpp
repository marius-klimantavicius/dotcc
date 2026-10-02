//type: fp
//options:  --c++11
// PR c++/56701
// { dg-require-effective-target c++11 }

struct A
{
  void f(){ A*&& a = this; }
};

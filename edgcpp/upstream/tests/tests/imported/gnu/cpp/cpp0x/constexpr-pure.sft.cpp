//type: fp
//options: --c++11
// { dg-do compile { target c++11 } }

struct A
{
  virtual void f() = 0;
};

struct B: A
{
  void f() { }
};

B b;

//type: fp
//options: --c++11
// PR c++/55972
// { dg-do compile { target c++11 } }

class C
{
  void f();
  int j = 10;
  int i = [this]() { return this->j; }();
};

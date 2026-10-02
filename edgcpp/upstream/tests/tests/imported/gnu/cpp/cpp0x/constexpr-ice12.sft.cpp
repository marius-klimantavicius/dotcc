//type: fp
//options: --c++11
// PR c++/58609
// { dg-do compile { target c++11 } }

struct A
{
  static constexpr int&& i = 0;
};

int j = A::i;

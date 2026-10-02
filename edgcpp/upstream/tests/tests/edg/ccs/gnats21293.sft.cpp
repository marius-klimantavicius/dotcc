//type:fn
//options::--clang_version 80000:--gnu_version 90000;cp:--microsoft_version 1900;cp
//options_all:--c++11

struct M
{
  M();
  M(M&);
};

template<typename T> struct W
{
  W();
  W(const W&) = default;
  T t;
};

W<M> w;

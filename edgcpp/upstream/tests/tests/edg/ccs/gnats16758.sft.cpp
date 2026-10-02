//type:cp
//options::--gnu_version 40903
//options_all:--c++11 -tused

struct S1
{
  static constexpr unsigned val1 = 5;
};
 
struct S2
{
  unsigned val2;
};
 
 
template <typename T>
struct S
{
  static constexpr S2 x = { S1::val1 };
  static constexpr unsigned y = x.val2;
};

auto x = S<int>::x;
auto y = S<int>::y;

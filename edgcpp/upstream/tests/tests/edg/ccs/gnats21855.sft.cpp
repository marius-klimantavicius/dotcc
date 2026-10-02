//type:cp
//options:--c++11:--c++17:--g++ --c++11:--g++ --c++17
//options_all:-W

struct S
{
  S(const S&&) = delete;
};

struct T
{
  T(volatile T&&) = delete;
};

struct U
{
  U(U&&) = delete;
};

struct V
{
};

void f(S p1 = {}, T p2 = {}, U p3 = {}, V p4 = {});

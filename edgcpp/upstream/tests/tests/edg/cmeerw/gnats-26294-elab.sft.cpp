//type:fp
//options:--c++20 -tused
struct A
{
  struct B
  { };

  struct D;
};

int A;

namespace ns
{
  struct A
  { };

  int A;

  struct C
  {
    friend struct ::A::B;
  };
}

struct ::A a;
struct ns::A nsa;

struct A::D
{
  void f() { };
} d;


using T = int;
template<typename T> ::T f(T);

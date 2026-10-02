//type:fn
//options::--g++:--clang;fp:--microsoft
//options_all:--c++17

struct A
{
  A(double d);
};

struct B : A
{
  B(short s, double d = 1.0);
  using A::A;
};

B b(5); // Ambiguous
B b2(5, 10); // Not ambiguous

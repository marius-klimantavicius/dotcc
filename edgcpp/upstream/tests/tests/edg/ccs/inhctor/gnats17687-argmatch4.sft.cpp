//type:fn
//options::--g++:--clang;fp:--microsoft
//options_all:--c++17

struct A
{
  A(double d, short s = 2);
};

struct B : A
{
  B(short s, double d = 1.0);
  using A::A;
};

B b(5); // Ambiguous
B b2(5, 10); // Ambiguous

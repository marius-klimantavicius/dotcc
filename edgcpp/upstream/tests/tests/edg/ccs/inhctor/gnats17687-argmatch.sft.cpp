//type:fn
//options::--g++:--clang;fp:--microsoft
//options_all:--c++17

struct A
{
  A(double d);
};

struct B : A
{
  B(short s);
  using A::A;
};

B b(5); // Ambiguous

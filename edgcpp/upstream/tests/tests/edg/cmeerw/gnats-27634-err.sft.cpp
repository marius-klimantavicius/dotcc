//type:fn
//options:--c++11

struct A
{
  int f();
  using pmf = int (A::*)();
};

struct B : A
{
  enum A
  { };

  struct N : A { };  // we actually should get an error here as well
  pmf p = &A::f;
};

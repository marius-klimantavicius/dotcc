//type:fn
//options_all:--microsoft_v 1903
struct Base
{
  virtual int f() { return 37; }
};
struct Derived : Base
{
  constexpr int f() { return 47; }
};

//type:fn
//options_all:--c++17

class A
{
  A(int);
};

struct A2 : A
{
  using A::A;
  friend void f();
};

struct B: A2
{
  using A2::A2;
};

void f()
{
  B b(42);
}

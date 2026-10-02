//type:cp
//options:--c++17

class A
{
  A(int);
  friend void f();
};

struct B: A
{
  using A::A;
};

void f()
{
  B b(42);
}

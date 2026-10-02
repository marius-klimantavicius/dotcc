//type:cp
//options:--c++17

class A
{
  template<typename T> A(T);
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

//type:cp
//options:--c++11:--c++17
//options_all:-tused

template <class T> struct A
{
  template <class U>
  A(U, U = []{ return 42; }());
};

struct B: A<int>
{
  using A::A;
};

B b(24);

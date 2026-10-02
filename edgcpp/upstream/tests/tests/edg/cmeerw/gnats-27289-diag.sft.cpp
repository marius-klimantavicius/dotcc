//type:fn
//options:--c++20 -A -w

template<typename T>
T h();

template<typename T>
struct A
{
  A &r;
  const int cm;
  int m;

  static typename A<T *>::type x;

  void f()
  {
    r.m = 1;
    r.cm = 1;                   // error
    r.A::m = 1;
    r.A::cm = 1;                // error

    x.m == 1;
    x.cm == 1;
    x.A::m = 1;
    x.A::cm = 1;                // error
  }
};

template<typename T>
struct A<T *>
{
  using type = A<T>;
};

extern A<int> &a;
auto v = (a.f(), 1);

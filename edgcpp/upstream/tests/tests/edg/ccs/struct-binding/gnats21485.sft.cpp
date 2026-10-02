//type:cp
//options:
//options_all:--c++20 -tused -W

template<class T>
void f()
{
  struct A {
    T x;
  };
  auto [sb] = A();
  [sb]{};
};

void g()
{
  f<decltype([]{})>();
}

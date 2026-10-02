//type:cp
//options:--c++11:--microsoft_version 1700
//options_all:-tused

template <typename... T> struct S_3
{
  template <typename... U> struct inner_1
  {
    template <typename... V> struct inner_2
    {
      static void f(T... ts, U... us, V... vs);
    };
  };
};

void test()
{
  S_3<int>::inner_1<>::inner_2<int>::f(0,0);
}

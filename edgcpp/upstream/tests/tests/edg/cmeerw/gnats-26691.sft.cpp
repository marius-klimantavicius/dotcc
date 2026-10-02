//type:fp
//options:--c++20 -tused -d-hash_stats

// Hash statistics shouldn't show anything above 8.

template<typename T>
struct C { };

template<int I>
void f1()
{
  {
    auto l = [] () { };
    C<decltype(l)> c;
  }
  {
    auto l = [] () { };
    C<decltype(l)> c;
  }
  {
    auto l = [] () { };
    C<decltype(l)> c;
  }
  {
    auto l = [] () { };
    C<decltype(l)> c;
  }
  {
    auto l = [] () { };
    C<decltype(l)> c;
  }
  {
    auto l = [] () { };
    C<decltype(l)> c;
  }
  {
    auto l = [] () { };
    C<decltype(l)> c;
  }
  {
    auto l = [] () { };
    C<decltype(l)> c;
  }
  {
    auto l = [] () { };
    C<decltype(l)> c;
  }
  {
    auto l = [] () { };
    C<decltype(l)> c;
  }
}

template<int I>
void f2()
{
  f1<10*I + 0>();
  f1<10*I + 1>();
  f1<10*I + 2>();
  f1<10*I + 3>();
  f1<10*I + 4>();
  f1<10*I + 5>();
  f1<10*I + 6>();
  f1<10*I + 7>();
  f1<10*I + 8>();
  f1<10*I + 9>();
}

template<int I>
void f3()
{
  f2<10*I + 0>();
  f2<10*I + 1>();
  f2<10*I + 2>();
  f2<10*I + 3>();
  f2<10*I + 4>();
  f2<10*I + 5>();
  f2<10*I + 6>();
  f2<10*I + 7>();
  f2<10*I + 8>();
  f2<10*I + 9>();
}

template void f3<0>();
template void f3<1>();

//type:fp
//options:--c++11:--c++20:--c++20 --gn 150100:--c++20 --clang_version 200100:--ms_c++20 --microsoft_version 1944

namespace minimal
{
  using INT = int;
  template<typename T> int f(T, INT &&);
  template<typename T> int f(T *, int &&);
  int i = f("", 0);
}

namespace cls_partial_ordering
{
  using INT = int;

  template<typename, typename>
  struct C;

  template<typename T>
  struct C<T, INT &>;

  template<typename T>
  struct C<T *, int &>
  { };

  C<int *, INT &> c;
}

#if __cpp_concepts
namespace typedef_more_constrained
{
  template<bool>
  struct A
  { };

  template<bool B1>
  struct D
  { };

  using Dtrue = D<true>;

  template<bool B3>
  void f(Dtrue const &) = delete;

  template<bool B3>
  void f(D<true> const &) requires true;

  template<bool B3>
  void g(Dtrue const &, A<B3>) = delete;

  template<bool B3>
  void g(D<true> const &, A<B3>) requires true;

  void foo(D<true> d, A<true> a1)
  {
    f<true>(d);
    g(d, a1);
  }
}

namespace typedef_more_constrained_minimal
{
  using INT = int;

  template<typename T>
  int f(INT const &) = delete;

  template<typename T>
  int f(int const &) requires true;

  int i = f<int>(0);
}
#endif

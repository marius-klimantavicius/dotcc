//type:fn
//options:--c++11:--c++11 --gn 130200:--c++11 --clang_version 180100
//options_all:-w

namespace functionally_equivalent_types
{
  template<typename> class C { };
  template<bool> class X { };

  template<int I> struct B
  {
    template<typename T> void f(const C<T> &, X<I == 2 && sizeof(T)==1>);
    template<typename T> void f(const C<T> &, X<I == 3 && sizeof(T)==1>);
  };

  template<int I> struct D : B<I>
  {
    using B<I>::f;
    template<typename T> void f(const C<T> &, X<I == 2 && sizeof(T)==1>);
  };

  void g(D<0> d, C<int> c)
  {
    d.f(c, {});
    auto p = &D<0>::f<int>;
  }
}

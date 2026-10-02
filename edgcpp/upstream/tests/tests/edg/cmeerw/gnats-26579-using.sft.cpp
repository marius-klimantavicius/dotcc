//type:fp
//options:--c++11:--c++11 --gn 130200:--c++11 --clang_version 180100
//options_all:-w

namespace identical_types
{
  template<typename> class C {};

  template<int I> struct B
  {
    template<typename T> void f(const C<T> &);
  };

  template<int I> struct D : B<I>
  {
    using B<I>::f;
    template<typename T> void f(const C<T> &);
  };

  void g(D<1> d, C<int> c)
  {
    d.f(c);
    auto p = &D<1>::f<int>;
  }
}

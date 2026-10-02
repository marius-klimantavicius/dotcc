//type:fn
//options:--c++17:--c++20:--ms_c++17:--ms_c++20

namespace dpdt_fn_type
{
  template<typename T>
  struct C
  { };

  template<typename T>
  T x(T);

  template<typename T, typename U, auto (&F)(T) = x<U> >
  void f(C<T>, C<U>) { }

  void g()
  {
#ifndef _MSC_VER
// There is an unrelated issue in Microsoft mode
    f(C<int>{}, C<int>{});      // works
#endif

    f(C<int>{}, C<long>{});     // fails
  }
}

namespace decltype_auto_deduction_from_non_paren_init
{
  struct B
  {
    int i = 0;
  };

  constexpr B b;

  struct C
  {
    static constexpr B b{};
  };

  template<decltype(auto) v = b>
  constexpr int f()
  {
    return v.i;
  }

  template<decltype(auto) v = C::b>
  constexpr int g()
  {
    return v.i;
  }

  static_assert(f<>() == 0);    // error in C++17 mode
  static_assert(g<>() == 0);    // error in C++17 mode
}

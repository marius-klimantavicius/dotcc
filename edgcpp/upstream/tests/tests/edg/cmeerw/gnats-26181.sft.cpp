//type:fp
//options:--c++20:--c++20 --g++ --gnu_version 120100:--c++20 --clang --clang_version 150000:--ms_c++20

namespace minimal
{
  template<typename ... T> int f(T ... t) requires(sizeof...(T) > 1);
  int i = f<int>(1, 2);
}

namespace template_head_requires
{
  template<typename ... TT> requires(sizeof...(TT) == 0)
  void f0(TT ... tt);

  template<typename ... TT> requires(sizeof...(TT) == 1)
  void f1(TT ... tt);

  template<typename ... TT> requires(sizeof...(TT) == 2)
  void f2(TT ... tt);

  template<typename ... TT> requires(sizeof...(TT) == 3)
  void f3(TT ... tt);

  void foo()
  {
    f0();
    f0<>();

    f1(1);
    f1<>(1);
    f1<int>(1);

    f2(1, 2u);
    f2<>(1, 2u);
    f2<int>(1, 2u);
    f2<int, unsigned>(1, 2u);

    f3(1, 2u, 'c');
    f3<>(1, 2u, 'c');
    f3<int>(1, 2u, 'c');
    f3<int, unsigned>(1, 2u, 'c');
    f3<int, unsigned, char>(1, 2u, 'c');
  }
}

namespace trailing_requires
{
  template<typename ... TT>
  void f0(TT ... tt) requires(sizeof...(TT) == 0);

  template<typename ... TT>
  void f1(TT ... tt) requires(sizeof...(TT) == 1);

  template<typename ... TT>
  void f2(TT ... tt) requires(sizeof...(TT) == 2);

  template<typename ... TT>
  void f3(TT ... tt) requires(sizeof...(TT) == 3);

  void foo()
  {
    f0();
    f0<>();

    f1(1);
    f1<>(1);
    f1<int>(1);

    f2(1, 2u);
    f2<>(1, 2u);
    f2<int>(1, 2u);
    f2<int, unsigned>(1, 2u);

    f3(1, 2u, 'c');
    f3<>(1, 2u, 'c');
    f3<int>(1, 2u, 'c');
    f3<int, unsigned>(1, 2u, 'c');
    f3<int, unsigned, char>(1, 2u, 'c');
  }
}

namespace failed_constraints
{
  template<typename ... T>
  constexpr int f(T ... t)
  {
    return 0;
  }

  template<typename ... T>
  constexpr int f(T ... t) requires (sizeof ... (T) == 2)
  {
    return 2;
  }

  template<typename ... T> requires (sizeof ... (T) == 3)
  constexpr int f(T ... t)
  {
    return 3;
  }

  static_assert(f<>() == 0);
  static_assert(f<>(1) == 0);
  static_assert(f<int>(1) == 0);

  static_assert(f<>(1, 2) == 2);
  static_assert(f<int>(1, 2) == 2);
  static_assert(f<int, int>(1, 2) == 2);

  static_assert(f<>(1, 2, 3) == 3);
  static_assert(f<int>(1, 2, 3) == 3);
  static_assert(f<int, int>(1, 2, 3) == 3);
  static_assert(f<int, int, int>(1, 2, 3) == 3);
}

//type:fp
//options:--c++20:--c++20 --g++:--c++20 --clang:--ms_c++20

namespace minimal
{
  int g(int);
  template<typename ... T> int f(T ... t) requires requires { g(t ...); };
  int i = f(1);
}

namespace requires_params
{
  int f(int);

  template<typename ... TT> int g(TT ... tt)
    requires requires (TT ... uu) { f(uu ...); };

  template<typename ... TT> int h(TT ... tt)
    requires requires (TT ... uu) { f(tt + uu ...); };

  int i = g(1) + h(1);
}

namespace check_size
{
  template<typename ... TT> int g0(TT ... tt)
    requires (sizeof ... (tt) == 0);

  template<typename ... TT> int g1(TT ... tt)
    requires (sizeof ... (tt) == 1);

  template<typename ... TT> int g2(TT ... tt)
    requires (sizeof ... (tt) == 2);

  int i0 = g0();
  int i1 = g1(1);
  int i2 = g2(1, 2);
}

namespace fold_expression
{
  template<typename ... TT>
  constexpr bool f(TT ... tt) requires ((sizeof(tt) >= sizeof(int)) && ...)
  {
    return true;
  }

  static_assert(f());
  static_assert(f(1));
  static_assert(f(1, 2));
  static_assert(f(1, 2, 3));
}

namespace fold_expression_with_requires_params
{
  template<bool B>
  struct C
  { };

  template<>
  struct C<true>
  {
    static constexpr bool value = true;
  };

  template<typename ... TT>
  constexpr bool f(TT ... tt) requires
    ((sizeof(tt) >= sizeof(int)) && ...) ||
    requires {
      requires ((sizeof(tt) == sizeof(int)) && ...);
    } ||
    requires (TT ... uu) {
      C<((sizeof(tt) >= sizeof(int)) && ...)>::value;
    } ||
    requires (TT ... uu) {
      C<((sizeof(uu) >= sizeof(int)) && ...)>::value;
    } ||
    requires (TT ... uu) {
      C<((sizeof(tt) == sizeof(uu)) && ...)>::value;
      C<((sizeof(tt) >= sizeof(int)) && ...)>::value;
      C<((sizeof(uu) >= sizeof(int)) && ...)>::value;
    } ||
    requires (TT ... uu) {
      requires ((sizeof(uu) == sizeof(int)) && ...);
    } ||
    requires (TT ... uu) {
      requires ((sizeof(tt) == sizeof(int)) && ...);
    } ||
    ((sizeof(tt) >= sizeof(int)) && ...)
  {
    return true;
  }

  template<typename ... TT>
  constexpr bool f(TT ... tt)
  {
    return false;
  }

  static_assert(f());
  static_assert(f(1));
  static_assert(f(1, 2));
  static_assert(f(1, 2, 3));

  static_assert(!f('a'));
  static_assert(!f(1, 'a'));
  static_assert(!f('a', 2, 3));
}

#if !defined(_MSC_VER) && !defined(__clang__)
// When implementing the resolution of CWG2369 (which we do, except for MSVC
// and clang modes), check that function parameter types are not substituted
// for an unsatisfied trailing requires clause
namespace substitute_after_constraint_checking
{
  template<bool B>
  struct C
  {
    static_assert(B);
  };

  template<typename T>
  void g(T, typename C<sizeof(T) == 0>::type) requires false;

  template<typename T, typename U>
  int g(T, U);

  int i = g(1, 2);
}

namespace substitute_on_demand
{
  template<typename ... T>
  struct A
  { };

  template<unsigned> struct B { };
  template<> struct B<0> { static int m; };

  template<bool B>
  struct C
  {
    static_assert(B && !B);
  };

  template<typename ... T, typename ... U>
  void g(A<U ...> a, T ... t, typename C<sizeof(U) < 0>::type ... c)
    requires requires { B<sizeof ... (t)>::m; sizeof ... (c); };

  template<typename ... T> int g(A<T ...>, long, long);
  template<typename ... T> int g(A<T ...>, long, long, long, long);

  int i1 = g<int>(A<int>(), 2, 3);
  int i2 = g<int, long>(A<int, long>(), 2, 3, 4, 5);
}
#endif

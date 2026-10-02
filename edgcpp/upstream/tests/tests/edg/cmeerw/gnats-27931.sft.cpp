//type:fp
//options:--c++20 -A:--c++20 --gn 140200:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1942
//options_all:-w -tused

namespace minimal
{
  template<int I> constexpr int v = I;
  template<int ... Is> struct C { };
  template<int I> constexpr int i =
    []<int ... Is>(C<Is...>) requires requires { (1 + ... + v<Is>); } {
      return (1 + ... + v<Is>);
    } (C<1>());
  static_assert(i<0> == 2);
}

namespace minimal_friend
{
  struct B {
    static constexpr bool v = true;
  };
  template<typename = void>
  struct C {
    template<typename U>
    friend constexpr bool f(C, U) requires requires { U::v; } { return true; }
    template<typename U>
    friend constexpr bool f(C, U) requires requires { U::x; } { return false; }
  };
  static_assert(f(C<>{}, B{}));
}

namespace lambda_inside_var_template
{
  template<int I>
  constexpr int v = I;

  template<int ... Is>
  struct C
  { };

  template<int I>
  constexpr int i1 =
    []<int ... Is>(C<Is...>) requires requires { (1 + ... + v<Is>); }
    {
      return (0 + ... + v<Is>);
    } (C<1>());

  static_assert(i1<0> == 1);

  template<int ... Is>
  constexpr int i2 =
    []<int I>(C<I>) requires requires { (1 + ... + v<Is>); }
    {
      return (0 + ... + v<Is>);
    } (C<0>());

  static_assert(i2<1> == 1);
}

namespace lambda_inside_var_template_and_class_template
{
  template<int I>
  constexpr int v = I;

  template<int ... Is>
  struct C
  { };

  template<int J>
  struct D
  {
    template<int I>
    static constexpr int i1 =
      []<int ... Is>(C<Is...>) requires requires { (1 + ... + v<Is>); }
      {
        return (0 + ... + v<Is>);
      } (C<1>());

    template<int ... Is>
    static constexpr int i2 =
      []<int I>(C<I>) requires requires { (1 + ... + v<Is>); }
      {
        return (0 + ... + v<Is>);
      } (C<0>());
  };

  static_assert(D<2>::i1<0> == 1);
  static_assert(D<2>::i2<1> == 1);
}

namespace function_template_inside_class_template
{
  template<int I>
  constexpr int v = I;

  template<int ... Is>
  struct C
  { };

  template<int I>
  struct B1
  {
    struct N
    {
      template<int ... Is>
      static constexpr int f(C<Is...>) requires requires { (1 + ... + v<Is>); }
      {
        return (0 + ... + v<Is>);
      }
    };
  };

  static_assert(B1<0>::N::f(C<1>()) == 1);

  template<int ... Is>
  struct B2
  {
    struct N
    {
      template<int I>
      static constexpr int f(C<I>) requires requires { (1 + ... + v<Is>); }
      {
        return (0 + ... + v<Is>);
      }
    };
  };

  static_assert(B2<1>::N::f(C<0>()) == 1);
}

namespace friend_requires_clauses
{
  template<typename, typename>
  struct X;

  template<typename T>
  struct X<T, T>
  {
    static constexpr bool value = true;
  };

  template<typename>
  struct B
  { };

  template<typename T1>
  struct C
  {
    template<typename T2>
    struct D {
      template<typename T3> requires
        requires { X<T1, char[1]>(), X<T2, char[2]>(), X<T3, char[3]>(); }
      friend constexpr int g(D, B<T3>) requires
        requires { X<T1, char[1]>(), X<T2, char[2]>(), X<T3, char[3]>(); }
      { return 0; }

      template<typename T3> requires
        requires { X<T1, char[1]>(), X<T2, char[2]>(), X<T3, char[4]>(); }
      friend constexpr int g(D, B<T3>) requires
        requires { X<T1, char[1]>(), X<T2, char[2]>(), X<T3, char[3]>(); }
      { return -1; }

      template<typename T3> requires
        requires { X<T1, char[1]>(), X<T2, char[2]>(), X<T3, char[3]>(); }
      friend constexpr int g(D, B<T3>) requires
        requires { X<T1, char[1]>(), X<T2, char[2]>(), X<T3, char[4]>(); }
      { return -2; }
    };
  };

  template<typename T2>
  struct D {
    template<typename T3> requires
        requires { X<T2, char[2]>(), X<T3, char[3]>(); }
    friend constexpr int g(D, B<T3>) requires
        requires { X<T2, char[2]>(), X<T3, char[3]>(); }
    { return 0; }

    template<typename T3> requires
        requires { X<T2, char[2]>(), X<T3, char[4]>(); }
    friend constexpr int g(D, B<T3>) requires
        requires { X<T2, char[2]>(), X<T3, char[3]>(); }
    { return -1; }

    template<typename T3> requires
        requires { X<T2, char[2]>(), X<T3, char[3]>(); }
    friend constexpr int g(D, B<T3>) requires
        requires { X<T2, char[2]>(), X<T3, char[4]>(); }
    { return -2; }
  };

  struct A
  {
    template<typename T3> requires
      requires { X<T3, char[3]>(); }
    friend constexpr int g(A, B<T3>) requires
      requires { X<T3, char[3]>(); }
    { return 0; }

    template<typename T3> requires
      requires { X<T3, char[4]>(); }
    friend constexpr int g(A, B<T3>) requires
      requires { X<T3, char[3]>(); }
    { return -1; }

    template<typename T3> requires
      requires { X<T3, char[3]>(); }
    friend constexpr int g(A, B<T3>) requires
      requires { X<T3, char[4]>(); }
    { return -2; }
  };

  static_assert(g(C<char[1]>::D<char[2]>(), B<char[3]>()) == 0);
  static_assert(g(D<char[2]>(), B<char[3]>()) == 0);
  static_assert(g(A(), B<char[3]>()) == 0);
}

namespace friend_pack
{
  template<typename T, int I>
  constexpr T v = I;

  template<int ... Is>
  struct C
  {
    template<typename ... Us>
    friend constexpr bool f(C, Us ...)
      // only expanding enclosing packs works, but can't expand Us
      requires requires { (1 + ... + v<int, Is>); }
    { return true; }
  };

  static_assert(f(C<1, 2>{}, 3, 4));
}

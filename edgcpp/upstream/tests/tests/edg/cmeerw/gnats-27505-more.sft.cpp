//type:fp
//options:--c++20:--c++20 --gn 140200:--c++20 --clang_version 190100

namespace incomplete_type_in_requires_clause
{
  template<typename U>
  struct A
  {
    template<typename T> requires (sizeof(U) > 1)
    static constexpr bool tmpl1(T t)
    { return true; }

    template<typename T> requires (sizeof(U) <= 1)
    static constexpr bool tmpl1(T t)
    { return false; }

    template<typename T>
    static constexpr bool tmpl2(T t) requires (sizeof(U) > 1)
    { return true; }

    template<typename T>
    static constexpr bool tmpl2(T t) requires (sizeof(U) <= 1)
    { return false; }

    static constexpr bool nontmpl() requires (sizeof(U) > 1)
    { return true; }

    static constexpr bool nontmpl() requires (sizeof(U) <= 1)
    { return false; }
  };

  struct B;

  A<B> ab;
  A<char> ac;

  struct B
  { char arr[2]; };

  static_assert(A<B>::tmpl1(1));
  static_assert(A<B>::tmpl2(1));
  static_assert(A<B>::nontmpl());

  static_assert(!A<char>::tmpl1(1));
  static_assert(!A<char>::tmpl2(1));
  static_assert(!A<char>::nontmpl());
}

namespace shortcut_substitution_into_constraints_tmpl
{
  template<typename T>
  struct X
  {
    static_assert(sizeof(T) == 1);
  };

  template<typename T>
  struct C
  {
    template<typename U>
    static void f(U u) requires (sizeof(U) == 1) && X<T>::value;
    static int f(long);

    template<typename U>
    static void g(U u) requires (sizeof(T) == 1) && X<U>::value;
    static int g(long);
  };

  int i = C<int>::f(1) + C<int>::g(2);
}

namespace shortcut_substitution_into_constraints_nontmpl
{
  template<typename T>
  struct X
  {
    static_assert(sizeof(T) == 1);
  };

  template<typename T>
  struct C
  {
    template<typename U>
    struct D
    {
      static void f(U u) requires (sizeof(U) == 1) && X<T>::value;
      static int f(long);

      static void g(U u) requires (sizeof(T) == 1) && X<U>::value;
      static int g(long);
    };
  };

  int i = C<int>::D<int>::f(1) + C<int>::D<int>::g(2);
}

namespace friend_substitution_fold
{
  template<int I, int J>
  concept X = I == J;

  template<typename ... TT>
  struct C1
  {
    template<int I>
    struct D
    {
      friend void operator +(D, int) requires X<I, (TT{1} + ...)>
      { }

      friend void g(D, long) requires false
      { }
    };
  };

  template<int I>
  struct C2
  {
    template<typename ... TT>
    struct D
    {
      friend void operator +(D, int) requires X<I, (TT{1} + ...)>
      { }

      friend void g(D, long) requires false
      { }
    };
  };

  void f(C1<int, int>::D<2> d12, C2<2>::D<int, int> d22)
  {
    d12 + 1;
    d22 + 1;
  }
}

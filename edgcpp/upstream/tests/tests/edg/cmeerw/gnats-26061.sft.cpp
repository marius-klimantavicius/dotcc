//type:fp
//options:--c++20:--ms_c++20 --microsoft_version 1934

namespace minimal
{
  template<unsigned N> concept C = N > 1;
  template<typename> struct D
  {
    template<unsigned N> requires C<N>
    friend int f(D, const char (&)[N]);
  };
  int i = f(D<int>{}, "a");
}

namespace constrained_friend
{
  template<bool B>
  concept C = B;

  template<bool B>
  struct A
  { };

  template<typename T>
  struct D
  {
    template<bool B> requires C<B>
    bool friend operator == (D const &, A<B>)
    { return true; }
  };

  void foo(D<int> d, A<true> a)
  {
    d == a;
  }
}

namespace pack_expansion_in_constraint
{
  template<bool ... B>
  concept C = ( ... || B );

  template<bool ... B>
  struct A
  { };

  template<typename T>
  struct D
  {
    template<bool ... B> requires C<B ...>
    bool friend operator == (D const &, A<B ...>)
    { return true; }
  };

  template<typename T>
  constexpr bool has_equality = requires (D<int> d, T t) { d == t; };

  void foo(D<int> d, A<true> a1, A<false, true> a2,
           A<false, false, true> a3)
  {
    d == a1;
    d == a2;
    d == a3;

    static_assert(!has_equality<A<>>);

    static_assert( has_equality<A<true >>);
    static_assert(!has_equality<A<false>>);

    static_assert( has_equality<A<false, true>>);
    static_assert( has_equality<A<true,  false>>);
    static_assert( has_equality<A<true,  true>>);
    static_assert(!has_equality<A<false, false>>);
  }
}

namespace tmpl_fn_matching
{
  template<bool B>
  concept C = B;

  template<bool B>
  struct A
  { };

  template<typename T>
  struct D;

  template<bool B> requires C<B>
  bool operator == (D<int> const &, A<B>)
  { return true; }

  template<typename T>
  struct D
  {
    template<bool B> requires C<B>
    bool friend operator == (D const &, A<B>);
  };

  void foo(D<int> d, A<true> a)
  {
    d == a;
  }
}

namespace tmpl_fn_matching_friend_first
{
  template<bool B>
  concept C = B;

  template<bool B>
  struct A
  { };

  template<typename T>
  struct D
  {
    template<bool B> requires C<B>
    bool friend operator == (D const &, A<B>);
  };

  template<bool B> requires C<B>
  bool operator == (D<int> const &, A<B>)
  { return true; }

  void foo(D<int> d, A<true> a)
  {
    d == a;
  }
}

namespace constrainted_type
{
  template<typename T, typename U>
  concept C = sizeof(T) == sizeof(U);

  template<typename U>
  struct A
  { };

  template<typename T>
  struct D
  {
    template<C<char> U>
    bool friend operator == (D const &, A<U>)
    { return true; }
  };

  template<C<int> U>
  bool operator == (D<int> const &, A<U>)
  { return true; }

  template<typename T>
  constexpr bool has_equality = requires (D<int> d, T t) { d == t; };

  static_assert( has_equality<A<int>>);
  static_assert( has_equality<A<char>>);
  static_assert(!has_equality<A<void>>);
}

//type:fp
//options:--c++17

namespace more_specialized_pack_1
{
  template<typename ...Types> struct tuple;

  template<typename T>
  struct X
  { };

  template<typename P>
  struct C
  {
    static constexpr int value = 0;
  };

  template<typename P1, typename ... PP>
  struct C<tuple<P1, X<PP> ...>>
  {
    static constexpr int value = 1;
  };

  template<typename Q1, typename ... QQ>
  struct C<tuple<Q1, QQ ...>>
  {
    static constexpr int value = 2;
  };

  static_assert(C<tuple<X<char>>>::value == 1);
  static_assert(C<tuple<X<char>, X<int>>>::value == 1);
  static_assert(C<tuple<char>>::value == 1);
  static_assert(C<tuple<char, int>>::value == 2);
  static_assert(C<char>::value == 0);
}

namespace more_specialized_pack_2
{
  template<typename ...Types> struct tuple;

  template<typename Tuple>
  struct X1 {
    static const unsigned value = 0;
  };

  template<typename Head, typename ...Tail>
  struct X1<tuple<Head, Tail...> > {
    static const unsigned value = 1;
  };

  template<typename Head, typename ...Tail>
  struct X1<tuple<Head, Tail&...> > {
    static const unsigned value = 2;
  };

  template<typename Head, typename ...Tail>
  struct X1<tuple<Head&, Tail&...> > {
    static const unsigned value = 3;
  };

  static_assert(X1<tuple<int>>::value == 2);
  static_assert(X1<tuple<int, int>>::value == 1);
}

namespace more_specialized_pack_3
{
  template<typename T>
  struct D
  { };

  template<typename T>
  struct C
  { };

  template<typename T, typename ... Args>
  struct C<T (*) (D<Args> ...)>
  {
    static constexpr int value = 1;
  };

  template<typename ... Args>
  struct C<void (*) (D<Args> ...)>
  {
    static constexpr int value = 2;
  };

  static_assert(C<void (*) (D<int>)>::value == 2);
  static_assert(C<void (*) (D<int>, D<char>)>::value == 2);
  static_assert(C<int (*) (D<int>)>::value == 1);
  static_assert(C<int (*) (D<int>, D<char>)>::value == 1);
}

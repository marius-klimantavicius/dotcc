//type:fp
//options:--c++20:--ms_c++20 --microsoft_version 1936:--c++20 --gn 130200:--c++20 --clang_version 170001

namespace minimal
{
  template<typename>
  concept C1 = true;
  template<typename T>
  concept C = (C1<typename T::A> || C1<typename T::B>);
  struct D {
    using A = int;
  };
  static_assert(C<D>);
}

namespace disjunction
{
  template<typename T>
  concept C = true;

  template<typename T>
  concept X = (C<typename T::A> || C<typename T::B>);

  template<typename T>
  concept X2 = C<typename T::A> || C<typename T::B>;

  struct DA
  {
    using A = int;
  };

  struct DB
  {
    using B = int;
  };

  static_assert(X<DA> && X<DB>);
  static_assert(X2<DA> && X2<DB>);
}

namespace conjunction
{
  template<typename T>
  concept C = false;

  template<typename T, typename U>
  concept X = (C<T> && C<typename U::B>);

  template<typename T, typename U>
  concept X2 = C<T> && C<typename U::B>;

  template<typename T = void>
  struct DA
  { };

  template<typename T = void>
  struct DB
  {
    using B = T;
    T t;
  };

  static_assert(!X<DA<>, DB<>>);
  static_assert(!X2<DA<>, DB<>>);
}

namespace nested
{
  template<typename T>
  concept C = true;

  template<typename T>
  concept X = ((true && C<typename T::A>) || (true && C<typename T::B>));

  struct DA
  {
    using A = int;
  };

  struct DB
  {
    using B = int;
  };

  static_assert(X<DA> && X<DB>);
}

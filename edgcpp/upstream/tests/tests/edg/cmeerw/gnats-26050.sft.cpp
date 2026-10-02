//type:fp
//options:--c++20:--ms_c++20

template<typename T1, typename T2>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;

namespace minimal
{
  template<typename T, typename U> concept C = true;
  template<C<int> T> struct D {
    D(T) { };
  };
  D d{1};
  static_assert(is_same_v<decltype(d), D<int>>);
}

namespace minimal_variadic
{
  template<typename T, typename ... U> concept C = true;
  template<C<int> T> struct D {
    D(T) { };
  };
  D d{1};
  static_assert(is_same_v<decltype(d), D<int>>);
}

namespace minimal_requires
{
  template<typename T, typename U> concept C = true;
  template<typename T> requires C<T, int>
  struct D {
    D(T) { };
  };
  D d{1};
  static_assert(is_same_v<decltype(d), D<int>>);
}

namespace minimal_requires_variadic
{
  template<typename T, typename ... U> concept C = true;
  template<typename T> requires C<T, int>
  struct D {
    D(T) { };
  };
  D d{1};
  static_assert(is_same_v<decltype(d), D<int>>);
}

namespace concept_substitution
{
  template<typename T, typename U> concept C = is_same_v<T, U>;

  static_assert(C<int, int>);
  static_assert(!C<int, char>);

  template<C<int> T> struct D {
    D(T) { };
  };
  D d{1};
  static_assert(is_same_v<decltype(d), D<int>>);
}

namespace concept_substitution_variadic
{
  template<typename T, typename ... U> concept C = (is_same_v<T, U> || ...);

  static_assert(C<int, int>);
  static_assert(C<int, char, int>);
  static_assert(!C<int, char>);
  static_assert(!C<int, char, short>);

  template<C<int, short> T> struct D {
    D(T) { };
  };
  D d{1};
  static_assert(is_same_v<decltype(d), D<int>>);
}

//type:fp
//options:--c++20:--ms_c++20 --microsoft_version=1927

template<typename T1, typename T2>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;

namespace designated_init
{
  template<typename T, typename U>
  struct C
  {
    char c;
    T t;
    int i;
    U u;
  };

  static_assert(is_same_v<decltype(C{ ' ', 2, 3, 4L }),
                C<int, long>>);

  static_assert(is_same_v<decltype(C{ .c = ' ', .t = 2, .i = 3, .u = 4L }),
                C<int, long>>);

  static_assert(is_same_v<decltype(C{ .t = 2, .u = 4L }),
                C<int, long>>);
}

namespace designated_init_with_base_class_pack
{
  template<typename T>
  struct B
  { };

  template<typename ... T>
  struct C
  { };

  template<typename U, typename ... T>
  struct A : B<T> ... {
    U u;
    C<T ...> c;
  };

  static_assert(is_same_v<decltype(A{ .u = 1, .c = C<char, short>{} }),
                A<int, char, short>>);
  static_assert(is_same_v<decltype(A{ .u = 'a', .c = C<int, short>{} }),
                A<char, int, short>>);
}

namespace designated_init_with_multiple_base_class_packs
{
  template<typename T>
  struct B1
  { };

  template<typename T>
  struct B2
  { };

  template<typename ... T>
  struct C
  { };

  template<typename U, typename ... T>
  struct A : B1<T> ..., B2<T> ... {
    U u;
    C<T ...> c;
  };

  static_assert(is_same_v<decltype(A{ .u = 1, .c = C<char, short>{} }),
                A<int, char, short>>);
  static_assert(is_same_v<decltype(A{ .u = 'a', .c = C<int, short>{} }),
                A<char, int, short>>);
}

namespace no_copy_deduction_guide_for_designators
{
  template<typename T>
  struct C
  {
    T t;
  };

  static_assert(is_same_v<decltype(C{ C<int>{} }), C<int>>);
  static_assert(is_same_v<decltype(C{ .t = C<int>{} }), C<C<int>>>);

  template<typename T>
  struct D
  {
    int i;
    T t;
  };

  static_assert(is_same_v<decltype(D{ D<int>{} }), D<int>>);
  static_assert(is_same_v<decltype(D{ {}, D<int>{} }), D<D<int>>>);
  static_assert(is_same_v<decltype(D{ .t = D<int>{} }), D<D<int>>>);
}

namespace no_user_declared_deduction_guides_for_designators
{
  template<typename T>
  struct Y
  {
    int i;
    T t;
    int j;
  };

  template<typename T>
  Y(int, T) -> Y<T>;

  static_assert(is_same_v<decltype(Y{ 1, 'b' }), Y<char>>);
}

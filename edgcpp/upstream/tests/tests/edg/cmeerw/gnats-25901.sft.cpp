//type:fp
//options:--c++20:--ms_c++20 --microsoft_version=1927

template<typename T1, typename T2>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;

namespace minimal
{
  template<typename T> struct C {
    template<typename U> C(U);
  };
  template<typename T> C(T) -> C<T *>;
  template<typename T> using A = C<T>;
  A a{1};

  static_assert(is_same_v<decltype(a), A<int *>>);
}

namespace cwg_2664
{
  template<class S1, class S2> struct C
  {
    C(...);
  };

  template<class T1> C(T1) -> C<T1, T1>;
  template<class T1, class T2> C(T1, T2) -> C<T1 *, T2>;

  template<class V1, class V2> using A = C<V1, V2>;

  C c1{""};
  A a1{""};

  C c2{"", 1};
  A a2{"", 1};
}

namespace partial_deduce_return_result
{
  template<typename T1, typename T2>
  struct C
  {
    C(...);
  };

  template<typename T, typename U>
  C(T, U) -> C<T, U *>;

  template<typename U>
  using A = C<int, U>;

  C c(1L, "");
  A a(1, "");
}

namespace deduce_return
{
  template<typename T1, typename T2>
  struct C
  {
    C(...);
  };

  template<typename T>
  struct X
  {
    using type = T;
  };

  template<typename T>
  C(T, T) -> C<T, typename X<T>::type>;

  template<typename T>
  using A = C<int, T>;

  C c1(1, 2);

  A a1(2, 3);
  A a2(3, 4L);
}

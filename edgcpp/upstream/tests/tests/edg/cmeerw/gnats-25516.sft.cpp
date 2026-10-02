//type:fp
//options:--c++23:--c++23 --gn 150200:--ms_c++latest --microsoft_version 1951
//options_all:-w -tused

template<typename, typename>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;

namespace minimal
{
  template<typename T>
  struct B {
    B(T);
  };
  template<typename T>
  struct D : B<T> {
    using B<T>::B;
  };
  D d(1);

  static_assert(is_same_v<decltype(d), D<int>>);
}

static_assert(__cpp_deduction_guides >= 202207L);

namespace simple_implicit_guides
{
  template<typename T>
  struct B
  {
    B(T);
  };

  template<typename ... Ts>
  struct BP
  {
    BP(Ts ...);
  };

  template<typename T>
  struct D : B<T *>
  {
    using B<T *>::B;
  };

  template<typename ... Ts>
  struct DP : BP<Ts * ...>
  {
    using BP<Ts * ...>::BP;
  };


  template<typename ... Args>
  concept is_ctorable = requires { D(Args() ...); };

  template<typename ... Args>
  concept is_ctorable_pack = requires { DP(Args() ...); };


  static_assert(!is_ctorable<int>);
  static_assert( is_ctorable<int *>);

  static_assert(!is_ctorable_pack<int>);
  static_assert(!is_ctorable_pack<int, int *>);
  static_assert(!is_ctorable_pack<int *, int>);
  static_assert( is_ctorable_pack<int *>);
  static_assert( is_ctorable_pack<int *, int *>);
  static_assert( is_ctorable_pack<char *, short *>);

  D d("");
  static_assert(is_same_v<decltype(d), D<const char>>);

  DP dp1("");
  static_assert(is_same_v<decltype(dp1), DP<const char>>);

  DP dp2("", "");
  static_assert(is_same_v<decltype(dp2), DP<const char, const char>>);
}

namespace add_new_base_guides
{
  template<typename T>
  struct B
  {
    B(int);
  };

  template<typename T>
  struct D : B<T>
  {
    using B<T>::B;
  };

  B(short) -> B<short>;

  D d1(1);
  static_assert(is_same_v<decltype(d1), D<short>>);

  B(int) -> B<int>;

  D d2(1);
  static_assert(is_same_v<decltype(d2), D<int>>);
}

namespace add_new_base_template_guides
{
  template<typename T>
  struct B
  {
    B(int, int);
  };

  template<typename T>
  struct D : B<T>
  {
    using B<T>::B;
  };

  template<typename T>
  B(T, short) -> B<short>;

  D d1(1, 1);
  static_assert(is_same_v<decltype(d1), D<short>>);

  template<typename T>
  B(T, int) -> B<int>;

  D d2(1, 1);
  static_assert(is_same_v<decltype(d2), D<int>>);
}

namespace compatibility
{
  template<typename T>
  struct B
  {
    B(T);
  };

  template<typename T>
  struct D : B<T *>
  {
    D(T);
    using B<T *>::B;
  };

  D d("");
  static_assert(is_same_v<decltype(d), D<const char>>);
}

namespace overload
{
  template<typename T>
  struct B
  {
    constexpr B()
    { }

    constexpr B(T)
    { }
  };

  template<typename T>
  struct D : B<T>
  {
    using B<T>::B;

    constexpr D(T)
      : i(1)
    { }

    int i = 0;
  };


  static_assert(D(0).i == 1);
}

namespace non_tmpl_base
{
  struct B
  { };

  template<typename T>
  struct D : B
  {
    using B::B;
    D(T);
  };

  D d(1);
}

namespace member_template
{
  template<typename T>
  struct O
  {
    template<typename U>
    struct N
    {
      N(U, int);
    };

    template<typename U>
    N(U, long) -> N<long>;

    N(const char *, long) -> N<const char *>;

    template<typename U>
    struct D : N<U>
    {
      using N<U>::N;
    };
  };

  O<int>::D d1i(1, 1);
  O<long>::D d1l(1, 1);
  static_assert(is_same_v<decltype(d1i), O<int>::D<int>>);
  static_assert(is_same_v<decltype(d1l), O<long>::D<int>>);

  O<int>::D d2i(1, 2L);
  O<long>::D d2l(1, 2L);
  static_assert(is_same_v<decltype(d2i), O<int>::D<long>>);
  static_assert(is_same_v<decltype(d2l), O<long>::D<long>>);

  O<int>::D d3i("", 3L);
  O<long>::D d3l("", 3L);
  static_assert(is_same_v<decltype(d3i), O<int>::D<const char *>>);
  static_assert(is_same_v<decltype(d3l), O<long>::D<const char *>>);
}

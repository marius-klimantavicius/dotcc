//type:fp
//options:--c++17:--c++20:--ms_c++20

namespace minimal
{
  struct X {
    X(const char *);
  };
  struct C {
    X x;
  };
  static_assert(!__is_constructible(C, int), "Unexpected");
}

static constexpr bool aggr_paren_init =
#if defined(__cpp_aggregate_paren_init)
  true;
#else
  false;
#endif

namespace aggr_member_ctor
{
  struct X
  {
    X(const char *) noexcept;
  };

  struct C
  {
    X x;
  };

  static_assert(!__is_constructible(C, int));
  static_assert(!__is_nothrow_constructible(C, int));
  static_assert(!__is_trivially_constructible(C, int));
  static_assert( __is_constructible(C, const char *) == aggr_paren_init);
  static_assert( __is_nothrow_constructible(C, const char *) == aggr_paren_init);
  static_assert(!__is_trivially_constructible(C, const char *));
}

namespace aggr_member_explicit_ctor
{
  struct X
  {
    explicit X(const char *) noexcept;
  };

  struct C
  {
    X x;
  };

  static_assert(!__is_constructible(C, int));
  static_assert(!__is_nothrow_constructible(C, int));
  static_assert(!__is_trivially_constructible(C, int));
  static_assert(!__is_constructible(C, const char *));
  static_assert(!__is_nothrow_constructible(C, const char *));
  static_assert(!__is_trivially_constructible(C, const char *));
}

namespace aggr_member_trivial
{
  struct C
  {
    int i;
  };

  static_assert( __is_constructible(C, int) == aggr_paren_init);
  static_assert( __is_nothrow_constructible(C, int) == aggr_paren_init);
  static_assert( __is_trivially_constructible(C, int) == aggr_paren_init);
  static_assert(!__is_constructible(C, const char *));
  static_assert(!__is_nothrow_constructible(C, const char *));
  static_assert(!__is_trivially_constructible(C, const char *));
}

namespace non_aggr
{
  struct C
  {
    explicit C(int) noexcept;
  };

  static_assert( __is_constructible(C, int));
  static_assert( __is_nothrow_constructible(C, int));
  static_assert(!__is_trivially_constructible(C, int));
  static_assert(!__is_constructible(C, const char *));
  static_assert(!__is_nothrow_constructible(C, const char *));
  static_assert(!__is_trivially_constructible(C, const char *));
}

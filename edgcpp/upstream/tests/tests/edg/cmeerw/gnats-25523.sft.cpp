//type:fp
//options:--c++20:--c++20 --g++:--c++11 --microsoft_version=1800:--ms_c++20 --microsoft_version=1932

constexpr bool is_msvc =
#if defined(_MSC_VER)
    true;
#else
    false;
#endif

constexpr bool is_gcc =
#if defined(__GNUC__)
    true;
#else
    false;
#endif

struct expl_deleted
{
  expl_deleted(int);

  expl_deleted() = delete;
};

static_assert(__has_trivial_constructor(expl_deleted) != is_msvc, "OK");
static_assert(__has_nothrow_constructor(expl_deleted) != is_msvc, "OK");
static_assert(__is_trivial(expl_deleted) == is_gcc, "OK");
static_assert(__is_literal_type(expl_deleted) != is_msvc, "OK");


struct impl_deleted : expl_deleted
{ };

static_assert(__has_trivial_constructor(impl_deleted) != is_msvc, "OK");
static_assert(__has_nothrow_constructor(impl_deleted) != is_msvc, "OK");
static_assert(__is_trivial(impl_deleted) == is_gcc, "OK");
static_assert(__is_literal_type(impl_deleted) != is_msvc, "OK");


struct defaulted_deleted : expl_deleted
{
  defaulted_deleted(int);

  defaulted_deleted() = default;
};

static_assert(__has_trivial_constructor(defaulted_deleted) != is_msvc, "OK");
static_assert(__has_nothrow_constructor(defaulted_deleted) != is_msvc, "OK");
static_assert(__is_trivial(defaulted_deleted) == is_gcc, "OK");
static_assert(__is_literal_type(defaulted_deleted) != is_msvc, "OK");

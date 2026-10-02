//type:fp
//options:--target linux_i686 --clang_version 190100:--target linux_x86_64 --clang_version 190100:--target linux_armv7 --clang_version 190100:--target linux_aarch64 --clang_version 190100:--target linux_i686 --gn 140200:--target linux_x86_64 --gn 140200:--target linux_armv7 --gn 140200:--target linux_aarch64 --gn 140200
//options_all:--c++20 -w

typedef __builtin_va_list __gnuc_va_list;
typedef __gnuc_va_list va_list;

template<typename, typename>
constexpr bool is_same = false;

template<typename T>
constexpr bool is_same<T, T> = true;

#if defined(__x86_64)
static_assert(sizeof(va_list) == 24);
static_assert(alignof(va_list) == 8);
#elif defined(__aarch64__)
static_assert(sizeof(va_list) == 32);
static_assert(alignof(va_list) == 8);
#else
static_assert(sizeof(va_list) == 4);
static_assert(alignof(va_list) == 4);
#endif


template<typename T>
concept is_array = requires (T &t) { t[0]; };

#if defined(__ARM_ARCH)
static_assert(!is_array<va_list>);
#else
static_assert(is_array<va_list>);
#endif

void f(int i, ...)
{
  va_list l;

  __builtin_va_start(l, i);
  int j = __builtin_va_arg(l, int);
  __builtin_vprintf("%s", l);
  __builtin_va_end(l);
}

void g(int i, ...)
{
  va_list l;

  __builtin_va_start(l, i);

#if !defined(__x86_64)
  va_list ll{l};
  ll = l;
#endif

#if defined(__clang__)
#if defined(__aarch64__)
  static_assert(is_same<decltype(l.__stack), void *>);
  static_assert(is_same<decltype(l.__gr_top), void *>);
  static_assert(is_same<decltype(l.__vr_top), void *>);
  static_assert(is_same<decltype(l.__gr_offs), int>);
  static_assert(is_same<decltype(l.__vr_offs), int>);
#elif defined(__arm__)
  static_assert(is_same<decltype(l.__ap), void *>);
#endif
#endif
}

namespace std
{
  void adl_fn(int i, ...) = delete;
}

void adl_fn(long i, ...);

void h(int i)
{
  va_list l;
  adl_fn(1, l);
}

namespace std
{
  typedef __builtin_va_list __va_list;
  typedef __builtin_va_list __va_list;
}

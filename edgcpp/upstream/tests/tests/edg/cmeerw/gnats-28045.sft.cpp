//type:fp
//options:--c++20 --gn 150100:--c++20 --gn 140200:--c++20 --clang_version 190100

static_assert(__builtin_clzg((unsigned char) 0, -1) == -1);
static_assert(__builtin_clzg((unsigned short) 0, -1) == -1);
static_assert(__builtin_clzg(0u, -1) == -1);
static_assert(__builtin_clzg(0ul, -1) == -1);
static_assert(__builtin_clzg(0ull, -1) == -1);

static_assert(__builtin_clzg((unsigned char) 4, 0) == 5);
static_assert(__builtin_clzg((unsigned short) 4, 0) == 13);

static_assert(__builtin_clzg(0u, 0) == 0);
static_assert(__builtin_clzg(0u, 1) == 1);
static_assert(__builtin_clzg(1u) == 31);
static_assert(__builtin_clzg(1u, 0) == 31);
static_assert(__builtin_clzg(2u) == 30);
static_assert(__builtin_clzg(2u, 0) == 30);
static_assert(__builtin_clzg(3u) == 30);
static_assert(__builtin_clzg(3u, 0) == 30);
static_assert(__builtin_clzg(4u) == 29);
static_assert(__builtin_clzg(4u, 0) == 29);

static_assert(__builtin_clzg(0ull, 0) == 0);
static_assert(__builtin_clzg(0ull, 1) == 1);
static_assert(__builtin_clzg(1ull) == 63);
static_assert(__builtin_clzg(1ull, 0) == 63);
static_assert(__builtin_clzg(2ull) == 62);
static_assert(__builtin_clzg(2ull, 0) == 62);
static_assert(__builtin_clzg(3ull) == 62);
static_assert(__builtin_clzg(3ull, 0) == 62);
static_assert(__builtin_clzg(4ull) == 61);
static_assert(__builtin_clzg(4ull, 0) == 61);

static_assert(__builtin_clzg(0x1000'0000'0000'0000ull, 0) == 3);
static_assert(__builtin_clzg(0x2000'0000'0000'0000ull, 0) == 2);
static_assert(__builtin_clzg(0x4000'0000'0000'0000ull, 0) == 1);
static_assert(__builtin_clzg(0x8000'0000'0000'0000ull, 0) == 0);

static_assert(__builtin_ctzg(0u, 0) == 0);
static_assert(__builtin_ctzg(0u, 1) == 1);
static_assert(__builtin_ctzg(1u) == 0);
static_assert(__builtin_ctzg(1u, 0) == 0);
static_assert(__builtin_ctzg(2u) == 1);
static_assert(__builtin_ctzg(2u, 0) == 1);
static_assert(__builtin_ctzg(3u) == 0);
static_assert(__builtin_ctzg(3u, 0) == 0);
static_assert(__builtin_ctzg(4u) == 2);
static_assert(__builtin_ctzg(4u, 0) == 2);

static_assert(__builtin_ctzg(0ull, 0) == 0);
static_assert(__builtin_ctzg(0ull, 1) == 1);
static_assert(__builtin_ctzg(1ull) == 0);
static_assert(__builtin_ctzg(1ull, 0) == 0);
static_assert(__builtin_ctzg(2ull) == 1);
static_assert(__builtin_ctzg(2ull, 0) == 1);
static_assert(__builtin_ctzg(3ull) == 0);
static_assert(__builtin_ctzg(3ull, 0) == 0);
static_assert(__builtin_ctzg(4ull) == 2);
static_assert(__builtin_ctzg(4ull, 0) == 2);

static_assert(__builtin_ctzg(0x1000'0000'0000'0000ull, 0) == 60);
static_assert(__builtin_ctzg(0x2000'0000'0000'0000ull, 0) == 61);
static_assert(__builtin_ctzg(0x4000'0000'0000'0000ull, 0) == 62);
static_assert(__builtin_ctzg(0x8000'0000'0000'0000ull, 0) == 63);

static_assert(__builtin_popcountg(1u) == 1);
static_assert(__builtin_popcountg(1ull) == 1);
static_assert(__builtin_popcountg(2u) == 1);
static_assert(__builtin_popcountg(2ull) == 1);
static_assert(__builtin_popcountg(3u) == 2);
static_assert(__builtin_popcountg(3ull) == 2);

static_assert(__builtin_popcountg(0x1000'0000'0000'0000ull) == 1);
static_assert(__builtin_popcountg(0x2000'0000'0000'0000ull) == 1);
static_assert(__builtin_popcountg(0x4000'0000'0000'0000ull) == 1);
static_assert(__builtin_popcountg(0x8000'0000'0000'0000ull) == 1);

#ifndef __clang__
static_assert(__builtin_ffsg(0) == 0);
static_assert(__builtin_ffsg(0ll) == 0);
static_assert(__builtin_ffsg(1) == 1);
static_assert(__builtin_ffsg(1ll) == 1);
static_assert(__builtin_ffsg(2) == 2);
static_assert(__builtin_ffsg(2ll) == 2);
static_assert(__builtin_ffsg(3) == 1);
static_assert(__builtin_ffsg(3ll) == 1);
static_assert(__builtin_ffsg(-1) == 1);
static_assert(__builtin_ffsg(-1ll) == 1);
static_assert(__builtin_ffsg(-2) == 2);
static_assert(__builtin_ffsg(-2ll) == 2);
static_assert(__builtin_ffsg(-3) == 1);
static_assert(__builtin_ffsg(-3ll) == 1);

static_assert(__builtin_ffsg(0x1000'0000'0000'0000ll) == 61);
static_assert(__builtin_ffsg(0x2000'0000'0000'0000ll) == 62);
static_assert(__builtin_ffsg(0x4000'0000'0000'0000ll) == 63);

static_assert(__builtin_parityg(1u) == 1);
static_assert(__builtin_parityg(1ull) == 1);
static_assert(__builtin_parityg(2u) == 1);
static_assert(__builtin_parityg(2ull) == 1);
static_assert(__builtin_parityg(3u) == 0);
static_assert(__builtin_parityg(3ull) == 0);

static_assert(__builtin_parityg(0x1000'0000'0000'0000ull) == 1);
static_assert(__builtin_parityg(0x2000'0000'0000'0000ull) == 1);
static_assert(__builtin_parityg(0x4000'0000'0000'0000ull) == 1);
static_assert(__builtin_parityg(0x8000'0000'0000'0000ull) == 1);
#endif

template<typename T>
void f(T t)
{
  __builtin_clzg(t);
  __builtin_clzg(t, 0);
  __builtin_ctzg(t);
  __builtin_ctzg(t, 0);
  __builtin_popcountg(t);

#ifndef __clang__
  __builtin_ffsg(t);
  __builtin_parityg(t);
#endif
}

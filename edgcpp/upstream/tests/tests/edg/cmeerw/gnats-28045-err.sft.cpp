//type:fn
//options:--c++20 --gn 150100:--c++20 --clang_version 190100

enum ES : int { };
enum EU : unsigned int { };
struct S { };

void f(unsigned int ui, int i, ES es, EU eu, S s)
{
  __builtin_clzg(i);
  __builtin_clzg(es);
  __builtin_clzg(eu);
  __builtin_clzg(s);
  __builtin_clzg(1u, es);
  __builtin_clzg(1u, eu);

  __builtin_ctzg(i);
  __builtin_ctzg(es);
  __builtin_ctzg(eu);
  __builtin_ctzg(s);
  __builtin_ctzg(1u, es);
  __builtin_ctzg(1u, eu);

  __builtin_popcountg(i);
  __builtin_popcountg(es);
  __builtin_popcountg(eu);
  __builtin_popcountg(s);

  __builtin_parityg(i);
  __builtin_parityg(es);
  __builtin_parityg(eu);
  __builtin_parityg(s);

  __builtin_ffsg(ui);
  __builtin_ffsg(es);
  __builtin_ffsg(eu);
  __builtin_ffsg(s);

  static_assert(__builtin_clz(ui) == 0);

  static_assert(__builtin_clzg(ui, 0) == 0);
  static_assert(__builtin_clzg(ui, i) == 0);
  static_assert(__builtin_clzg(0u, i) == 0);
  static_assert(__builtin_clzg(1u, i) == 0);

  static_assert(__builtin_ctz(ui) == 0);

  static_assert(__builtin_ctzg(ui, 0) == 0);
  static_assert(__builtin_ctzg(ui, i) == 0);
  static_assert(__builtin_ctzg(0u, i) == 0);
  static_assert(__builtin_ctzg(1u, i) == 0);

  static_assert(__builtin_popcountg(ui) == 0);

#ifndef __clang__
  static_assert(__builtin_parityg(ui) == 0);

  static_assert(__builtin_ffsg(i) == 0);
#endif
}

static_assert(__builtin_clzg() == 0);
static_assert(__builtin_clzg(1u, 2, 3) == 0);

static_assert(__builtin_ctzg() == 0);
static_assert(__builtin_ctzg(1u, 2, 3) == 0);

static_assert(__builtin_popcountg() == 0);
static_assert(__builtin_popcountg(1u, 2) == 0);

static_assert(__builtin_parityg() == 0);
static_assert(__builtin_parityg(1u, 2) == 0);

static_assert(__builtin_ffsg() == 0);
static_assert(__builtin_ffsg(1, 2) == 0);

//type: fn
//options: --c++23
# 0 "./cpp23/ext-floating3.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp23/ext-floating3.C"
# 9 "./cpp23/ext-floating3.C"
# 1 "./cpp23/ext-floating.h" 1


namespace std
{

  using float16_t = _Float16;


  using float32_t = _Float32;


  using float64_t = _Float64;


  using float128_t = _Float128;


  using bfloat16_t = decltype (0.0bf16);

  template<typename T, T v> struct integral_constant {
    static constexpr T value = v;
  };
  typedef integral_constant<bool, false> false_type;
  typedef integral_constant<bool, true> true_type;
  template<class T, class U>
  struct is_same : std::false_type {};
  template <class T>
  struct is_same<T, T> : std::true_type {};
}
# 10 "./cpp23/ext-floating3.C" 2
# 20 "./cpp23/ext-floating3.C"
using namespace std;


float16_t f16i = 1.0f;
float16_t f16k = 1.0;
float16_t f16m = 1.0L;
float16_t f16o = 1.0Q;

float32_t f32i = 1.0f;
float32_t f32k = 1.0;
float32_t f32m = 1.0L;
float32_t f32o = 1.0Q;
float64_t f64i = 1.0f;
float64_t f64k = 1.0;
float64_t f64m = 1.0L;
float64_t f64o = 1.0Q;
float128_t f128i = 1.0f;
float128_t f128k = 1.0;
float128_t f128m = 1.0L;
float128_t f128o = 1.0Q;


constexpr float16_t f16x = 1.0F16;

constexpr float32_t f32x = 2.0F32;
constexpr float64_t f64x = 3.0F64;
constexpr float128_t f128x = 4.0F128;
constexpr float fx = 5.0f;
constexpr double dx = 6.0;
constexpr long double ldx = 7.0L;

constexpr int foo (float32_t) { return 1; }
constexpr int foo (float64_t) { return 2; }
constexpr int bar (float) { return 3; }
constexpr int bar (double) { return 4; }
constexpr int bar (long double) { return 5; }
constexpr int baz (float32_t) { return 6; }
constexpr int baz (float64_t) { return 7; }
constexpr int baz (float128_t) { return 8; }
constexpr int qux (float64_t) { return 9; }
constexpr int qux (float32_t) { return 10; }
constexpr int fred (long double) { return 11; }
constexpr int fred (double) { return 12; }
constexpr int fred (float) { return 13; }
constexpr int thud (float128_t) { return 14; }
constexpr int thud (float64_t) { return 15; }
constexpr int thud (float32_t) { return 16; }
struct S {
  constexpr operator float32_t () const { return 1.0f32; }
  constexpr operator float64_t () const { return 2.0f64; }
};
struct T {
  constexpr operator float64_t () const { return 3.0f64; }
  constexpr operator float32_t () const { return 4.0f32; }
};

void
test (S s, T t)
{

  foo (float16_t (1.0));

  static_assert (foo (float (2.0)) == 1);
  static_assert (foo (double (3.0)) == 2);
  constexpr double x (s);
  static_assert (x == 2.0);

  bar (f16x);

  static_assert (bar (f32x) == 3);
  static_assert (bar (f64x) == 4);
  bar (f128x);



  static_assert (bar (fx) == 3);
  static_assert (bar (dx) == 4);
  static_assert (bar (ldx) == 5);

  baz (f16x);

  static_assert (baz (f32x) == 6);
  static_assert (baz (f64x) == 7);
  static_assert (baz (f128x) == 8);
  static_assert (baz (fx) == 6);
  static_assert (baz (dx) == 7);
  static_assert (baz (ldx) == 8);

  qux (float16_t (1.0));

  static_assert (qux (float (2.0)) == 10);
  static_assert (qux (double (3.0)) == 9);
  constexpr double y (t);
  static_assert (y == 3.0);

  fred (f16x);

  static_assert (fred (f32x) == 13);
  static_assert (fred (f64x) == 12);
  fred (f128x);



  static_assert (fred (fx) == 13);
  static_assert (fred (dx) == 12);
  static_assert (fred (ldx) == 11);

  thud (f16x);

  static_assert (thud (f32x) == 16);
  static_assert (thud (f64x) == 15);
  static_assert (thud (f128x) == 14);
  static_assert (thud (fx) == 16);
  static_assert (thud (dx) == 15);
  static_assert (thud (ldx) == 14);
}

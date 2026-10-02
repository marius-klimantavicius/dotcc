//type: fp
//options: --c++23
# 0 "./cpp23/ext-floating11.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp23/ext-floating11.C"




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
# 6 "./cpp23/ext-floating11.C" 2





extern "C" void abort ();

using namespace std;

template <typename T, typename U>
int
foo (T x, U y) noexcept
{
  return 3;
}

int
main ()
{
  if (foo (0.0f, 0.0f) != 3)
    abort ();
  if (foo (0.0, 0.0) != 3)
    abort ();
  if (foo (0.0L, 0.0L) != 3)
    abort ();

  if (foo (0.0f16, 0.0f16) != 3)
    abort ();
  if (foo (0.0f, 0.0f16) != 3)
    abort ();


  if (foo (0.0f32, 0.0f32) != 3)
    abort ();
  if (foo (0.0f, 0.0f32) != 3)
    abort ();


  if (foo (0.0f64, 0.0f64) != 3)
    abort ();
  if (foo (0.0, 0.0f64) != 3)
    abort ();


  if (foo (0.0f128, 0.0f128) != 3)
    abort ();
  if (foo (0.0L, 0.0f128) != 3)
    abort ();


  if (foo (0.0bf16, 0.0bf16) != 3)
    abort ();
  if (foo (0.0f, 0.0bf16) != 3)
    abort ();


  if (foo (0.0f32x, 0.0f32x) != 3)
    abort ();
  if (foo (0.0, 0.0f32x) != 3)
    abort ();


  if (foo (0.0f64x, 0.0f64x) != 3)
    abort ();
  if (foo (0.0L, 0.0f64x) != 3)
    abort ();







}

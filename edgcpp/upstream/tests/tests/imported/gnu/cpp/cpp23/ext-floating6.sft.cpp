//type: fp
//options: --c++23
# 0 "./cpp23/ext-floating6.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp23/ext-floating6.C"




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
# 6 "./cpp23/ext-floating6.C" 2





using namespace std;

float foo (float x, float y, float z) { return x * y + z; }
double foo (double x, double y, double z) { return x * y + z; }
long double foo (long double x, long double y, long double z) { return x * y + z; }

float16_t foo (float16_t x, float16_t y, float16_t z) { return x * y + z; }


float32_t foo (float32_t x, float32_t y, float32_t z) { return x * y + z; }


float64_t foo (float64_t x, float64_t y, float64_t z) { return x * y + z; }


float128_t foo (float128_t x, float128_t y, float128_t z) { return x * y + z; }


bfloat16_t foo (bfloat16_t x, bfloat16_t y, bfloat16_t z) { return x * y + z; }

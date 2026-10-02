//type: fp
//options: --c++23
# 0 "./cpp23/ext-floating2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp23/ext-floating2.C"
# 9 "./cpp23/ext-floating2.C"
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
# 10 "./cpp23/ext-floating2.C" 2





using namespace std;

float fa = 1.0f;
float fb = (float) 1.0f;
float fc = 1.0;
float fd = (float) 1.0;
float fe = 1.0L;
float ff = (float) 1.0L;




double da = 1.0f;
double db = (double) 1.0f;
double dc = 1.0;
double dd = (double) 1.0;
double de = 1.0L;
double df = (double) 1.0L;




long double lda = 1.0f;
long double ldb = (long double) 1.0f;
long double ldc = 1.0;
long double ldd = (long double) 1.0;
long double lde = 1.0L;
long double ldf = (long double) 1.0L;
# 56 "./cpp23/ext-floating2.C"
float16_t f16a = 1.0F16;
float16_t f16b = (float16_t) 1.0F16;

float16_t f16c = 1.0F32;
float16_t f16d = (float16_t) 1.0F32;


float16_t f16e = 1.0F64;
float16_t f16f = (float16_t) 1.0F64;


float16_t f16g = 1.0F128;
float16_t f16h = (float16_t) 1.0F128;

float16_t f16j = (float16_t) 1.0f;
float16_t f16l = (float16_t) 1.0;
float16_t f16n = (float16_t) 1.0L;






float32_t f32a = 1.0F16;
float32_t f32b = (float32_t) 1.0F16;

float32_t f32c = 1.0F32;
float32_t f32d = (float32_t) 1.0F32;

float32_t f32e = 1.0F64;
float32_t f32f = (float32_t) 1.0F64;


float32_t f32g = 1.0F128;
float32_t f32h = (float32_t) 1.0F128;


float32_t f32i = 1.0f;

float32_t f32j = (float32_t) 1.0f;
float32_t f32l = (float32_t) 1.0;
float32_t f32n = (float32_t) 1.0L;






float64_t f64a = 1.0F16;
float64_t f64b = (float64_t) 1.0F16;


float64_t f64c = 1.0F32;
float64_t f64d = (float64_t) 1.0F32;

float64_t f64e = 1.0F64;
float64_t f64f = (float64_t) 1.0F64;

float64_t f64g = 1.0F128;
float64_t f64h = (float64_t) 1.0F128;


float64_t f64i = 1.0f;

float64_t f64j = (float64_t) 1.0f;

float64_t f64k = 1.0;

float64_t f64l = (float64_t) 1.0;
float64_t f64n = (float64_t) 1.0L;






float128_t f128a = 1.0F16;
float128_t f128b = (float128_t) 1.0F16;


float128_t f128c = 1.0F32;
float128_t f128d = (float128_t) 1.0F32;


float128_t f128e = 1.0F64;
float128_t f128f = (float128_t) 1.0F64;

float128_t f128g = 1.0F128;
float128_t f128h = (float128_t) 1.0F128;

float128_t f128i = 1.0f;

float128_t f128j = (float128_t) 1.0f;

float128_t f128k = 1.0;

float128_t f128l = (float128_t) 1.0;

float128_t f128m = 1.0L;

float128_t f128n = (float128_t) 1.0L;

//type: fp
//options: --c++23
# 0 "./cpp23/ext-floating1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp23/ext-floating1.C"




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
# 6 "./cpp23/ext-floating1.C" 2





using namespace std;

static_assert (!is_same<float, double>::value);
static_assert (!is_same<float, long double>::value);
static_assert (!is_same<double, long double>::value);
static_assert (is_same<decltype (0.0f), float>::value);
static_assert (is_same<decltype (0.0F), float>::value);
static_assert (is_same<decltype (0.0), double>::value);
static_assert (is_same<decltype (0.0l), long double>::value);
static_assert (is_same<decltype (0.0L), long double>::value);
static_assert (is_same<decltype (0.0f + 0.0F), float>::value);
static_assert (is_same<decltype (0.0F + 0.0f), float>::value);
static_assert (is_same<decltype (0.0 + 0.0), double>::value);
static_assert (is_same<decltype (0.0l + 0.0L), long double>::value);
static_assert (is_same<decltype (0.0L + 0.0l), long double>::value);







static_assert (!is_same<float, float16_t>::value);
static_assert (!is_same<double, float16_t>::value);
static_assert (!is_same<long double, float16_t>::value);
static_assert (is_same<decltype (0.0f16), float16_t>::value);
static_assert (is_same<decltype (0.0F16), float16_t>::value);
static_assert (is_same<decltype (0.0f16 + 0.0f16), float16_t>::value);
static_assert (is_same<decltype (0.0F16 + 0.0F16), float16_t>::value);


static_assert (!is_same<float, float32_t>::value);
static_assert (!is_same<double, float32_t>::value);
static_assert (!is_same<long double, float32_t>::value);
static_assert (!is_same<decltype (0.0f), float32_t>::value);
static_assert (!is_same<decltype (0.0F), float32_t>::value);
static_assert (is_same<decltype (0.0f32), float32_t>::value);
static_assert (is_same<decltype (0.0F32), float32_t>::value);
static_assert (!is_same<decltype (0.0f32), float>::value);
static_assert (!is_same<decltype (0.0F32), float>::value);
static_assert (is_same<decltype (0.0f32 + 0.0f32), float32_t>::value);
static_assert (is_same<decltype (0.0F32 + 0.0F32), float32_t>::value);


static_assert (!is_same<float, float64_t>::value);
static_assert (!is_same<double, float64_t>::value);
static_assert (!is_same<long double, float64_t>::value);
static_assert (!is_same<decltype (0.0), float64_t>::value);
static_assert (is_same<decltype (0.0f64), float64_t>::value);
static_assert (is_same<decltype (0.0F64), float64_t>::value);
static_assert (!is_same<decltype (0.0f64), double>::value);
static_assert (!is_same<decltype (0.0F64), double>::value);
static_assert (is_same<decltype (0.0f64 + 0.0f64), float64_t>::value);
static_assert (is_same<decltype (0.0F64 + 0.0F64), float64_t>::value);


static_assert (!is_same<float, float128_t>::value);
static_assert (!is_same<double, float128_t>::value);
static_assert (!is_same<long double, float128_t>::value);
static_assert (!is_same<decltype (0.0l), float128_t>::value);
static_assert (!is_same<decltype (0.0L), float128_t>::value);
static_assert (is_same<decltype (0.0f128), float128_t>::value);
static_assert (is_same<decltype (0.0F128), float128_t>::value);
static_assert (!is_same<decltype (0.0f128), long double>::value);
static_assert (!is_same<decltype (0.0F128), long double>::value);
static_assert (is_same<decltype (0.0f128 + 0.0f128), float128_t>::value);
static_assert (is_same<decltype (0.0F128 + 0.0F128), float128_t>::value);
# 87 "./cpp23/ext-floating1.C"
static_assert (!is_same<float, bfloat16_t>::value);
static_assert (!is_same<double, bfloat16_t>::value);
static_assert (!is_same<long double, bfloat16_t>::value);
static_assert (is_same<decltype (0.0bf16), bfloat16_t>::value);
static_assert (is_same<decltype (0.0BF16), bfloat16_t>::value);
static_assert (is_same<decltype (0.0bf16 + 0.0bf16), bfloat16_t>::value);
static_assert (is_same<decltype (0.0BF16 + 0.0BF16), bfloat16_t>::value);


static_assert (!is_same<float, _Float32x>::value);
static_assert (!is_same<double, _Float32x>::value);
static_assert (!is_same<long double, _Float32x>::value);
static_assert (!is_same<decltype (0.0f), _Float32x>::value);
static_assert (!is_same<decltype (0.0F), _Float32x>::value);
static_assert (is_same<decltype (0.0f32x), _Float32x>::value);
static_assert (is_same<decltype (0.0F32x), _Float32x>::value);
static_assert (!is_same<decltype (0.0f32x), float>::value);
static_assert (!is_same<decltype (0.0F32x), float>::value);
static_assert (is_same<decltype (0.0f32x + 0.0f32x), _Float32x>::value);
static_assert (is_same<decltype (0.0F32x + 0.0F32x), _Float32x>::value);

static_assert (!is_same<float16_t, _Float32x>::value);
static_assert (!is_same<decltype (0.0f16), _Float32x>::value);
static_assert (!is_same<decltype (0.0F16), _Float32x>::value);
static_assert (!is_same<decltype (0.0f32x), float16_t>::value);
static_assert (!is_same<decltype (0.0F32x), float16_t>::value);


static_assert (!is_same<float32_t, _Float32x>::value);
static_assert (!is_same<decltype (0.0f32), _Float32x>::value);
static_assert (!is_same<decltype (0.0F32), _Float32x>::value);
static_assert (!is_same<decltype (0.0f32x), float32_t>::value);
static_assert (!is_same<decltype (0.0F32x), float32_t>::value);


static_assert (!is_same<float64_t, _Float32x>::value);
static_assert (!is_same<decltype (0.0f64), _Float32x>::value);
static_assert (!is_same<decltype (0.0F64), _Float32x>::value);
static_assert (!is_same<decltype (0.0f32x), float64_t>::value);
static_assert (!is_same<decltype (0.0F32x), float64_t>::value);


static_assert (!is_same<float128_t, _Float32x>::value);
static_assert (!is_same<decltype (0.0f128), _Float32x>::value);
static_assert (!is_same<decltype (0.0F128), _Float32x>::value);
static_assert (!is_same<decltype (0.0f32x), float128_t>::value);
static_assert (!is_same<decltype (0.0F32x), float128_t>::value);



static_assert (!is_same<float, _Float64x>::value);
static_assert (!is_same<double, _Float64x>::value);
static_assert (!is_same<long double, _Float64x>::value);
static_assert (!is_same<decltype (0.0), _Float64x>::value);
static_assert (is_same<decltype (0.0f64x), _Float64x>::value);
static_assert (is_same<decltype (0.0F64x), _Float64x>::value);
static_assert (!is_same<decltype (0.0f64x), double>::value);
static_assert (!is_same<decltype (0.0F64x), double>::value);
static_assert (is_same<decltype (0.0f64x + 0.0f64x), _Float64x>::value);
static_assert (is_same<decltype (0.0F64x + 0.0F64x), _Float64x>::value);

static_assert (!is_same<float16_t, _Float64x>::value);
static_assert (!is_same<decltype (0.0f16), _Float64x>::value);
static_assert (!is_same<decltype (0.0F16), _Float64x>::value);
static_assert (!is_same<decltype (0.0f64x), float16_t>::value);
static_assert (!is_same<decltype (0.0F64x), float16_t>::value);


static_assert (!is_same<float32_t, _Float64x>::value);
static_assert (!is_same<decltype (0.0f32), _Float64x>::value);
static_assert (!is_same<decltype (0.0F32), _Float64x>::value);
static_assert (!is_same<decltype (0.0f64x), float32_t>::value);
static_assert (!is_same<decltype (0.0F64x), float32_t>::value);


static_assert (!is_same<float64_t, _Float64x>::value);
static_assert (!is_same<decltype (0.0f64), _Float64x>::value);
static_assert (!is_same<decltype (0.0F64), _Float64x>::value);
static_assert (!is_same<decltype (0.0f64x), float64_t>::value);
static_assert (!is_same<decltype (0.0F64x), float64_t>::value);


static_assert (!is_same<float128_t, _Float64x>::value);
static_assert (!is_same<decltype (0.0f128), _Float64x>::value);
static_assert (!is_same<decltype (0.0F128), _Float64x>::value);
static_assert (!is_same<decltype (0.0f64x), float128_t>::value);
static_assert (!is_same<decltype (0.0F64x), float128_t>::value);
# 231 "./cpp23/ext-floating1.C"
static_assert (is_same<decltype (0.0f + 0.0), double>::value);
static_assert (is_same<decltype (0.0 + 0.0F), double>::value);
static_assert (is_same<decltype (0.0L + 0.0), long double>::value);
static_assert (is_same<decltype (0.0 + 0.0L), long double>::value);
static_assert (is_same<decltype (0.0L + 0.0f), long double>::value);
static_assert (is_same<decltype (0.0F + 0.0l), long double>::value);

static_assert (!is_same<float16_t, float32_t>::value);
static_assert (is_same<decltype (0.0f16 + 0.0f32), float32_t>::value);
static_assert (is_same<decltype (0.0F32 + 0.0F16), float32_t>::value);


static_assert (!is_same<float16_t, float64_t>::value);
static_assert (is_same<decltype (0.0f16 + 0.0f64), float64_t>::value);
static_assert (is_same<decltype (0.0F64 + 0.0F16), float64_t>::value);


static_assert (!is_same<float16_t, float128_t>::value);
static_assert (is_same<decltype (0.0f16 + 0.0f128), float128_t>::value);
static_assert (is_same<decltype (0.0F128 + 0.0F16), float128_t>::value);


static_assert (is_same<decltype (0.0f16 + 0.0f32x), _Float32x>::value);
static_assert (is_same<decltype (0.0F32x + 0.0F16), _Float32x>::value);


static_assert (is_same<decltype (0.0f16 + 0.0f64x), _Float64x>::value);
static_assert (is_same<decltype (0.0F64x + 0.0F16), _Float64x>::value);






static_assert (!is_same<float32_t, float64_t>::value);
static_assert (is_same<decltype (0.0f32 + 0.0f64), float64_t>::value);
static_assert (is_same<decltype (0.0F64 + 0.0F32), float64_t>::value);


static_assert (!is_same<float32_t, float128_t>::value);
static_assert (is_same<decltype (0.0f32 + 0.0f128), float128_t>::value);
static_assert (is_same<decltype (0.0F128 + 0.0F32), float128_t>::value);


static_assert (is_same<decltype (0.0f32 + 0.0f32x), _Float32x>::value);
static_assert (is_same<decltype (0.0F32x + 0.0F32), _Float32x>::value);


static_assert (is_same<decltype (0.0f32 + 0.0f64x), _Float64x>::value);
static_assert (is_same<decltype (0.0F64x + 0.0F32), _Float64x>::value);






static_assert (!is_same<float64_t, float128_t>::value);
static_assert (is_same<decltype (0.0f64 + 0.0f128), float128_t>::value);
static_assert (is_same<decltype (0.0F128 + 0.0F64), float128_t>::value);




static_assert (is_same<decltype (0.0f64 + 0.0f32x), float64_t>::value);
static_assert (is_same<decltype (0.0F32x + 0.0F64), float64_t>::value);


static_assert (is_same<decltype (0.0f64 + 0.0f64x), _Float64x>::value);
static_assert (is_same<decltype (0.0F64x + 0.0F64), _Float64x>::value);
# 308 "./cpp23/ext-floating1.C"
static_assert (is_same<decltype (0.0f128 + 0.0f32x), float128_t>::value);
static_assert (is_same<decltype (0.0F32x + 0.0F128), float128_t>::value);




static_assert (is_same<decltype (0.0f128 + 0.0f64x), float128_t>::value);
static_assert (is_same<decltype (0.0F64x + 0.0F128), float128_t>::value);






static_assert (!is_same<bfloat16_t, float32_t>::value);
static_assert (is_same<decltype (0.0bf16 + 0.0f32), float32_t>::value);
static_assert (is_same<decltype (0.0F32 + 0.0BF16), float32_t>::value);


static_assert (!is_same<bfloat16_t, float64_t>::value);
static_assert (is_same<decltype (0.0bf16 + 0.0f64), float64_t>::value);
static_assert (is_same<decltype (0.0F64 + 0.0BF16), float64_t>::value);


static_assert (!is_same<bfloat16_t, float128_t>::value);
static_assert (is_same<decltype (0.0bf16 + 0.0f128), float128_t>::value);
static_assert (is_same<decltype (0.0F128 + 0.0BF16), float128_t>::value);



static_assert (is_same<decltype (0.0f + 0.0f16), float>::value);
static_assert (is_same<decltype (0.0F16 + 0.0F), float>::value);


static_assert (is_same<decltype (0.0 + 0.0f16), double>::value);
static_assert (is_same<decltype (0.0F16 + 0.0), double>::value);


static_assert (is_same<decltype (0.0L + 0.0f16), long double>::value);
static_assert (is_same<decltype (0.0F16 + 0.0l), long double>::value);




static_assert (is_same<decltype (0.0f + 0.0f32), float32_t>::value);
static_assert (is_same<decltype (0.0F32 + 0.0F), float32_t>::value);


static_assert (is_same<decltype (0.0 + 0.0f32), double>::value);
static_assert (is_same<decltype (0.0F32 + 0.0), double>::value);


static_assert (is_same<decltype (0.0L + 0.0f32), long double>::value);
static_assert (is_same<decltype (0.0F32 + 0.0l), long double>::value);




static_assert (is_same<decltype (0.0f + 0.0f64), float64_t>::value);
static_assert (is_same<decltype (0.0F64 + 0.0F), float64_t>::value);


static_assert (is_same<decltype (0.0 + 0.0f64), float64_t>::value);
static_assert (is_same<decltype (0.0F64 + 0.0), float64_t>::value);


static_assert (is_same<decltype (0.0L + 0.0f64), long double>::value);
static_assert (is_same<decltype (0.0F64 + 0.0l), long double>::value);
# 389 "./cpp23/ext-floating1.C"
static_assert (is_same<decltype (0.0f + 0.0f128), float128_t>::value);
static_assert (is_same<decltype (0.0F128 + 0.0F), float128_t>::value);


static_assert (is_same<decltype (0.0 + 0.0f128), float128_t>::value);
static_assert (is_same<decltype (0.0F128 + 0.0), float128_t>::value);



static_assert (is_same<decltype (0.0L + 0.0f128), float128_t>::value);
static_assert (is_same<decltype (0.0F128 + 0.0l), float128_t>::value);
# 412 "./cpp23/ext-floating1.C"
static_assert (is_same<decltype (0.0 + 0.0bf16), double>::value);
static_assert (is_same<decltype (0.0BF16 + 0.0), double>::value);


static_assert (is_same<decltype (0.0L + 0.0bf16), long double>::value);
static_assert (is_same<decltype (0.0BF16 + 0.0l), long double>::value);



void foo (float) {}
void foo (double) {}
void foo (long double) {}

void foo (float16_t) {}


void foo (float32_t) {}


void foo (float64_t) {}


void foo (float128_t) {}


void foo (bfloat16_t) {}


void foo (_Float32x) {}


void foo (_Float64x) {}

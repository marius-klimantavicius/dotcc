//type: s
//options: --c++23
# 0 "./cpp23/ext-floating17.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp23/ext-floating17.C"
# 10 "./cpp23/ext-floating17.C"
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
# 11 "./cpp23/ext-floating17.C" 2





using namespace std;



float16_t f16c = 1.0F32;


float16_t f16e = 1.0F64;


float16_t f16g = 1.0F128;




float32_t f32e = 1.0F64;


float32_t f32g = 1.0F128;




float64_t f64g = 1.0F128;

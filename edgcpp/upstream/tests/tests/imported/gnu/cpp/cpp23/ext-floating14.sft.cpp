//type: rp
//options: --c++23
# 0 "./cpp23/ext-floating14.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp23/ext-floating14.C"





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
# 7 "./cpp23/ext-floating14.C" 2





template <typename T, typename F>
[[gnu::noipa]] T cvt (F f)
{
  return T (F (f));
}

int
main ()
{






  if (cvt <std::float16_t, signed char> (42) != (std::float16_t) 42
      || cvt <std::float16_t, signed char> (-42) != (std::float16_t) -42

      || cvt <std::float16_t, signed char> (0x7f) != (std::float16_t) 0x7f
      || cvt <std::float16_t, signed char> (-0x7f - 1) != (std::float16_t) (-0x7f - 1)

     )
    __builtin_abort ();
  if (cvt <std::float16_t, unsigned char> (42) != (std::float16_t) 42

      || cvt <std::float16_t, unsigned char> ((unsigned char) ~0) != (std::float16_t) ((unsigned char) ~0)

     )
    __builtin_abort ();
  if (cvt <std::float16_t, signed short> (42) != (std::float16_t) 42
      || cvt <std::float16_t, signed short> (-42) != (std::float16_t) -42

      || cvt <std::float16_t, signed short> (0x7fff) != (std::float16_t) 0x7fff
      || cvt <std::float16_t, signed short> (-0x7fff - 1) != (std::float16_t) (-0x7fff - 1)




     )
    __builtin_abort ();
  if (cvt <std::float16_t, unsigned short> (42) != (std::float16_t) 42
      || cvt <std::float16_t, unsigned short> (65504U) != (std::float16_t) 65504U)
    __builtin_abort ();
  if (cvt <std::float16_t, signed int> (42) != (std::float16_t) 42
      || cvt <std::float16_t, signed int> (-42) != (std::float16_t) -42




      || cvt <std::float16_t, signed int> (65504) != (std::float16_t) 65504
      || cvt <std::float16_t, signed int> (-65504) != (std::float16_t) -65504

     )
    __builtin_abort ();
  if (cvt <std::float16_t, unsigned int> (42) != (std::float16_t) 42U
      || cvt <std::float16_t, unsigned int> (65504U) != (std::float16_t) 65504U)
    __builtin_abort ();
  if (cvt <std::float16_t, signed long int> (42L) != (std::float16_t) 42L
      || cvt <std::float16_t, signed long int> (-42L) != (std::float16_t) -42L
      || cvt <std::float16_t, signed long int> (65504L) != (std::float16_t) 65504L
      || cvt <std::float16_t, signed long int> (-65504L) != (std::float16_t) -65504L)
    __builtin_abort ();
  if (cvt <std::float16_t, unsigned long int> (42UL) != (std::float16_t) 42UL
      || cvt <std::float16_t, unsigned long int> (65504UL) != (std::float16_t) 65504UL)
    __builtin_abort ();
  if (cvt <std::float16_t, signed long long int> (42LL) != (std::float16_t) 42LL
      || cvt <std::float16_t, signed long long int> (-42LL) != (std::float16_t) -42LL
      || cvt <std::float16_t, signed long long int> (65504LL) != (std::float16_t) 65504LL
      || cvt <std::float16_t, signed long long int> (-65504LL) != (std::float16_t) -65504LL)
    __builtin_abort ();
  if (cvt <std::float16_t, unsigned long long int> (42ULL) != (std::float16_t) 42ULL
      || cvt <std::float16_t, unsigned long long int> (65504ULL) != (std::float16_t) 65504ULL)
    __builtin_abort ();

  if (cvt <std::float16_t, signed __int128> (42LL) != (std::float16_t) (signed __int128) 42LL
      || cvt <std::float16_t, signed __int128> (-42LL) != (std::float16_t) (signed __int128) -42LL
      || cvt <std::float16_t, signed __int128> (65504LL) != (std::float16_t) (signed __int128) 65504LL
      || cvt <std::float16_t, signed __int128> (-65504LL) != (std::float16_t) (signed __int128) -65504LL)
    __builtin_abort ();
  if (cvt <std::float16_t, unsigned __int128> (42ULL) != (std::float16_t) (unsigned __int128) 42ULL
      || cvt <std::float16_t, unsigned __int128> (65504ULL) != (std::float16_t) (unsigned __int128) 65504ULL)
    __builtin_abort ();



  if (cvt <std::bfloat16_t, signed char> (42) != (std::bfloat16_t) 42
      || cvt <std::bfloat16_t, signed char> (-42) != (std::bfloat16_t) -42
      || cvt <std::bfloat16_t, signed char> (0x7f) != (std::bfloat16_t) 0x7f
      || cvt <std::bfloat16_t, signed char> (-0x7f - 1) != (std::bfloat16_t) (-0x7f - 1))
    __builtin_abort ();
  if (cvt <std::bfloat16_t, unsigned char> (42) != (std::bfloat16_t) 42
      || cvt <std::bfloat16_t, unsigned char> ((unsigned char) ~0) != (std::bfloat16_t) ((unsigned char) ~0))
    __builtin_abort ();
  if (cvt <std::bfloat16_t, signed short> (42) != (std::bfloat16_t) 42
      || cvt <std::bfloat16_t, signed short> (-42) != (std::bfloat16_t) -42
      || cvt <std::bfloat16_t, signed short> (0x7fff) != (std::bfloat16_t) 0x7fff
      || cvt <std::bfloat16_t, signed short> (-0x7fff - 1) != (std::bfloat16_t) (-0x7fff - 1))
    __builtin_abort ();
  if (cvt <std::bfloat16_t, unsigned short> (42) != (std::bfloat16_t) 42
      || cvt <std::bfloat16_t, unsigned short> ((unsigned short) ~0) != (std::bfloat16_t) ((unsigned short) ~0))
    __builtin_abort ();
  if (cvt <std::bfloat16_t, signed int> (42) != (std::bfloat16_t) 42
      || cvt <std::bfloat16_t, signed int> (-42) != (std::bfloat16_t) -42
      || cvt <std::bfloat16_t, signed int> (0x7fffffff) != (std::bfloat16_t) 0x7fffffff
      || cvt <std::bfloat16_t, signed int> (-0x7fffffff - 1) != (std::bfloat16_t) (-0x7fffffff - 1))
    __builtin_abort ();
  if (cvt <std::bfloat16_t, unsigned int> (42) != (std::bfloat16_t) 42U
      || cvt <std::bfloat16_t, unsigned int> (~0U) != (std::bfloat16_t) ~0U)
    __builtin_abort ();
  if (cvt <std::bfloat16_t, signed long int> (42L) != (std::bfloat16_t) 42L
      || cvt <std::bfloat16_t, signed long int> (-42L) != (std::bfloat16_t) -42L
      || cvt <std::bfloat16_t, signed long int> (0x7fffffffffffffffL) != (std::bfloat16_t) 0x7fffffffffffffffL
      || cvt <std::bfloat16_t, signed long int> (-0x7fffffffffffffffL - 1) != (std::bfloat16_t) (-0x7fffffffffffffffL - 1))
    __builtin_abort ();
  if (cvt <std::bfloat16_t, unsigned long int> (42UL) != (std::bfloat16_t) 42UL
      || cvt <std::bfloat16_t, unsigned long int> (~0UL) != (std::bfloat16_t) ~0UL)
    __builtin_abort ();
  if (cvt <std::bfloat16_t, signed long long int> (42LL) != (std::bfloat16_t) 42LL
      || cvt <std::bfloat16_t, signed long long int> (-42LL) != (std::bfloat16_t) -42LL
      || cvt <std::bfloat16_t, signed long long int> (0x7fffffffffffffffLL) != (std::bfloat16_t) 0x7fffffffffffffffLL
      || cvt <std::bfloat16_t, signed long long int> (-0x7fffffffffffffffLL - 1) != (std::bfloat16_t) (-0x7fffffffffffffffLL - 1))
    __builtin_abort ();
  if (cvt <std::bfloat16_t, unsigned long long int> (42ULL) != (std::bfloat16_t) 42ULL
      || cvt <std::bfloat16_t, unsigned long long int> (~0ULL) != (std::bfloat16_t) ~0ULL)
    __builtin_abort ();

  if (cvt <std::bfloat16_t, signed __int128> (42LL) != (std::bfloat16_t) (signed __int128) 42LL
      || cvt <std::bfloat16_t, signed __int128> (-42LL) != (std::bfloat16_t) (signed __int128) -42LL
      || cvt <std::bfloat16_t, signed __int128> (((signed __int128) ((~(unsigned __int128) 0) >> 1))) != (std::bfloat16_t) ((signed __int128) ((~(unsigned __int128) 0) >> 1))
      || cvt <std::bfloat16_t, signed __int128> (-((signed __int128) ((~(unsigned __int128) 0) >> 1)) - 1) != (std::bfloat16_t) (-((signed __int128) ((~(unsigned __int128) 0) >> 1)) - 1))
    __builtin_abort ();
  if (cvt <std::bfloat16_t, unsigned __int128> (42ULL) != (std::bfloat16_t) (unsigned __int128) 42ULL
      || cvt <std::bfloat16_t, unsigned __int128> (~(unsigned __int128) 0) != (std::bfloat16_t) (~(unsigned __int128) 0))
    __builtin_abort ();



  if (cvt <std::float32_t, signed char> (42) != (std::float32_t) 42
      || cvt <std::float32_t, signed char> (-42) != (std::float32_t) -42
      || cvt <std::float32_t, signed char> (0x7f) != (std::float32_t) 0x7f
      || cvt <std::float32_t, signed char> (-0x7f - 1) != (std::float32_t) (-0x7f - 1))
    __builtin_abort ();
  if (cvt <std::float32_t, unsigned char> (42) != (std::float32_t) 42
      || cvt <std::float32_t, unsigned char> ((unsigned char) ~0) != (std::float32_t) ((unsigned char) ~0))
    __builtin_abort ();
  if (cvt <std::float32_t, signed short> (42) != (std::float32_t) 42
      || cvt <std::float32_t, signed short> (-42) != (std::float32_t) -42
      || cvt <std::float32_t, signed short> (0x7fff) != (std::float32_t) 0x7fff
      || cvt <std::float32_t, signed short> (-0x7fff - 1) != (std::float32_t) (-0x7fff - 1))
    __builtin_abort ();
  if (cvt <std::float32_t, unsigned short> (42) != (std::float32_t) 42
      || cvt <std::float32_t, unsigned short> ((unsigned short) ~0) != (std::float32_t) ((unsigned short) ~0))
    __builtin_abort ();
  if (cvt <std::float32_t, signed int> (42) != (std::float32_t) 42
      || cvt <std::float32_t, signed int> (-42) != (std::float32_t) -42
      || cvt <std::float32_t, signed int> (0x7fffffff) != (std::float32_t) 0x7fffffff
      || cvt <std::float32_t, signed int> (-0x7fffffff - 1) != (std::float32_t) (-0x7fffffff - 1))
    __builtin_abort ();
  if (cvt <std::float32_t, unsigned int> (42) != (std::float32_t) 42U
      || cvt <std::float32_t, unsigned int> (~0U) != (std::float32_t) ~0U)
    __builtin_abort ();
  if (cvt <std::float32_t, signed long int> (42L) != (std::float32_t) 42L
      || cvt <std::float32_t, signed long int> (-42L) != (std::float32_t) -42L
      || cvt <std::float32_t, signed long int> (0x7fffffffffffffffL) != (std::float32_t) 0x7fffffffffffffffL
      || cvt <std::float32_t, signed long int> (-0x7fffffffffffffffL - 1) != (std::float32_t) (-0x7fffffffffffffffL - 1))
    __builtin_abort ();
  if (cvt <std::float32_t, unsigned long int> (42UL) != (std::float32_t) 42UL
      || cvt <std::float32_t, unsigned long int> (~0UL) != (std::float32_t) ~0UL)
    __builtin_abort ();
  if (cvt <std::float32_t, signed long long int> (42LL) != (std::float32_t) 42LL
      || cvt <std::float32_t, signed long long int> (-42LL) != (std::float32_t) -42LL
      || cvt <std::float32_t, signed long long int> (0x7fffffffffffffffLL) != (std::float32_t) 0x7fffffffffffffffLL
      || cvt <std::float32_t, signed long long int> (-0x7fffffffffffffffLL - 1) != (std::float32_t) (-0x7fffffffffffffffLL - 1))
    __builtin_abort ();
  if (cvt <std::float32_t, unsigned long long int> (42ULL) != (std::float32_t) 42ULL
      || cvt <std::float32_t, unsigned long long int> (~0ULL) != (std::float32_t) ~0ULL)
    __builtin_abort ();

  if (cvt <std::float32_t, signed __int128> (42LL) != (std::float32_t) (signed __int128) 42LL
      || cvt <std::float32_t, signed __int128> (-42LL) != (std::float32_t) (signed __int128) -42LL
      || cvt <std::float32_t, signed __int128> (((signed __int128) ((~(unsigned __int128) 0) >> 1))) != (std::float32_t) ((signed __int128) ((~(unsigned __int128) 0) >> 1))
      || cvt <std::float32_t, signed __int128> (-((signed __int128) ((~(unsigned __int128) 0) >> 1)) - 1) != (std::float32_t) (-((signed __int128) ((~(unsigned __int128) 0) >> 1)) - 1))
    __builtin_abort ();
  if (cvt <std::float32_t, unsigned __int128> (42ULL) != (std::float32_t) (unsigned __int128) 42ULL
      || cvt <std::float32_t, unsigned __int128> (~(unsigned __int128) 0) != (std::float32_t) (~(unsigned __int128) 0))
    __builtin_abort ();



  if (cvt <std::float64_t, signed char> (42) != (std::float64_t) 42
      || cvt <std::float64_t, signed char> (-42) != (std::float64_t) -42
      || cvt <std::float64_t, signed char> (0x7f) != (std::float64_t) 0x7f
      || cvt <std::float64_t, signed char> (-0x7f - 1) != (std::float64_t) (-0x7f - 1))
    __builtin_abort ();
  if (cvt <std::float64_t, unsigned char> (42) != (std::float64_t) 42
      || cvt <std::float64_t, unsigned char> ((unsigned char) ~0) != (std::float64_t) ((unsigned char) ~0))
    __builtin_abort ();
  if (cvt <std::float64_t, signed short> (42) != (std::float64_t) 42
      || cvt <std::float64_t, signed short> (-42) != (std::float64_t) -42
      || cvt <std::float64_t, signed short> (0x7fff) != (std::float64_t) 0x7fff
      || cvt <std::float64_t, signed short> (-0x7fff - 1) != (std::float64_t) (-0x7fff - 1))
    __builtin_abort ();
  if (cvt <std::float64_t, unsigned short> (42) != (std::float64_t) 42
      || cvt <std::float64_t, unsigned short> ((unsigned short) ~0) != (std::float64_t) ((unsigned short) ~0))
    __builtin_abort ();
  if (cvt <std::float64_t, signed int> (42) != (std::float64_t) 42
      || cvt <std::float64_t, signed int> (-42) != (std::float64_t) -42
      || cvt <std::float64_t, signed int> (0x7fffffff) != (std::float64_t) 0x7fffffff
      || cvt <std::float64_t, signed int> (-0x7fffffff - 1) != (std::float64_t) (-0x7fffffff - 1))
    __builtin_abort ();
  if (cvt <std::float64_t, unsigned int> (42) != (std::float64_t) 42U
      || cvt <std::float64_t, unsigned int> (~0U) != (std::float64_t) ~0U)
    __builtin_abort ();
  if (cvt <std::float64_t, signed long int> (42L) != (std::float64_t) 42L
      || cvt <std::float64_t, signed long int> (-42L) != (std::float64_t) -42L
      || cvt <std::float64_t, signed long int> (0x7fffffffffffffffL) != (std::float64_t) 0x7fffffffffffffffL
      || cvt <std::float64_t, signed long int> (-0x7fffffffffffffffL - 1) != (std::float64_t) (-0x7fffffffffffffffL - 1))
    __builtin_abort ();
  if (cvt <std::float64_t, unsigned long int> (42UL) != (std::float64_t) 42UL
      || cvt <std::float64_t, unsigned long int> (~0UL) != (std::float64_t) ~0UL)
    __builtin_abort ();
  if (cvt <std::float64_t, signed long long int> (42LL) != (std::float64_t) 42LL
      || cvt <std::float64_t, signed long long int> (-42LL) != (std::float64_t) -42LL
      || cvt <std::float64_t, signed long long int> (0x7fffffffffffffffLL) != (std::float64_t) 0x7fffffffffffffffLL
      || cvt <std::float64_t, signed long long int> (-0x7fffffffffffffffLL - 1) != (std::float64_t) (-0x7fffffffffffffffLL - 1))
    __builtin_abort ();
  if (cvt <std::float64_t, unsigned long long int> (42ULL) != (std::float64_t) 42ULL
      || cvt <std::float64_t, unsigned long long int> (~0ULL) != (std::float64_t) ~0ULL)
    __builtin_abort ();

  if (cvt <std::float64_t, signed __int128> (42LL) != (std::float64_t) (signed __int128) 42LL
      || cvt <std::float64_t, signed __int128> (-42LL) != (std::float64_t) (signed __int128) -42LL
      || cvt <std::float64_t, signed __int128> (((signed __int128) ((~(unsigned __int128) 0) >> 1))) != (std::float64_t) ((signed __int128) ((~(unsigned __int128) 0) >> 1))
      || cvt <std::float64_t, signed __int128> (-((signed __int128) ((~(unsigned __int128) 0) >> 1)) - 1) != (std::float64_t) (-((signed __int128) ((~(unsigned __int128) 0) >> 1)) - 1))
    __builtin_abort ();
  if (cvt <std::float64_t, unsigned __int128> (42ULL) != (std::float64_t) (unsigned __int128) 42ULL
      || cvt <std::float64_t, unsigned __int128> (~(unsigned __int128) 0) != (std::float64_t) (~(unsigned __int128) 0))
    __builtin_abort ();



  if (cvt <std::float128_t, signed char> (42) != (std::float128_t) 42
      || cvt <std::float128_t, signed char> (-42) != (std::float128_t) -42
      || cvt <std::float128_t, signed char> (0x7f) != (std::float128_t) 0x7f
      || cvt <std::float128_t, signed char> (-0x7f - 1) != (std::float128_t) (-0x7f - 1))
    __builtin_abort ();
  if (cvt <std::float128_t, unsigned char> (42) != (std::float128_t) 42
      || cvt <std::float128_t, unsigned char> ((unsigned char) ~0) != (std::float128_t) ((unsigned char) ~0))
    __builtin_abort ();
  if (cvt <std::float128_t, signed short> (42) != (std::float128_t) 42
      || cvt <std::float128_t, signed short> (-42) != (std::float128_t) -42
      || cvt <std::float128_t, signed short> (0x7fff) != (std::float128_t) 0x7fff
      || cvt <std::float128_t, signed short> (-0x7fff - 1) != (std::float128_t) (-0x7fff - 1))
    __builtin_abort ();
  if (cvt <std::float128_t, unsigned short> (42) != (std::float128_t) 42
      || cvt <std::float128_t, unsigned short> ((unsigned short) ~0) != (std::float128_t) ((unsigned short) ~0))
    __builtin_abort ();
  if (cvt <std::float128_t, signed int> (42) != (std::float128_t) 42
      || cvt <std::float128_t, signed int> (-42) != (std::float128_t) -42
      || cvt <std::float128_t, signed int> (0x7fffffff) != (std::float128_t) 0x7fffffff
      || cvt <std::float128_t, signed int> (-0x7fffffff - 1) != (std::float128_t) (-0x7fffffff - 1))
    __builtin_abort ();
  if (cvt <std::float128_t, unsigned int> (42) != (std::float128_t) 42U
      || cvt <std::float128_t, unsigned int> (~0U) != (std::float128_t) ~0U)
    __builtin_abort ();
  if (cvt <std::float128_t, signed long int> (42L) != (std::float128_t) 42L
      || cvt <std::float128_t, signed long int> (-42L) != (std::float128_t) -42L
      || cvt <std::float128_t, signed long int> (0x7fffffffffffffffL) != (std::float128_t) 0x7fffffffffffffffL
      || cvt <std::float128_t, signed long int> (-0x7fffffffffffffffL - 1) != (std::float128_t) (-0x7fffffffffffffffL - 1))
    __builtin_abort ();
  if (cvt <std::float128_t, unsigned long int> (42UL) != (std::float128_t) 42UL
      || cvt <std::float128_t, unsigned long int> (~0UL) != (std::float128_t) ~0UL)
    __builtin_abort ();
  if (cvt <std::float128_t, signed long long int> (42LL) != (std::float128_t) 42LL
      || cvt <std::float128_t, signed long long int> (-42LL) != (std::float128_t) -42LL
      || cvt <std::float128_t, signed long long int> (0x7fffffffffffffffLL) != (std::float128_t) 0x7fffffffffffffffLL
      || cvt <std::float128_t, signed long long int> (-0x7fffffffffffffffLL - 1) != (std::float128_t) (-0x7fffffffffffffffLL - 1))
    __builtin_abort ();
  if (cvt <std::float128_t, unsigned long long int> (42ULL) != (std::float128_t) 42ULL
      || cvt <std::float128_t, unsigned long long int> (~0ULL) != (std::float128_t) ~0ULL)
    __builtin_abort ();

  if (cvt <std::float128_t, signed __int128> (42LL) != (std::float128_t) (signed __int128) 42LL
      || cvt <std::float128_t, signed __int128> (-42LL) != (std::float128_t) (signed __int128) -42LL
      || cvt <std::float128_t, signed __int128> (((signed __int128) ((~(unsigned __int128) 0) >> 1))) != (std::float128_t) ((signed __int128) ((~(unsigned __int128) 0) >> 1))
      || cvt <std::float128_t, signed __int128> (-((signed __int128) ((~(unsigned __int128) 0) >> 1)) - 1) != (std::float128_t) (-((signed __int128) ((~(unsigned __int128) 0) >> 1)) - 1))
    __builtin_abort ();
  if (cvt <std::float128_t, unsigned __int128> (42ULL) != (std::float128_t) (unsigned __int128) 42ULL
      || cvt <std::float128_t, unsigned __int128> (~(unsigned __int128) 0) != (std::float128_t) (~(unsigned __int128) 0))
    __builtin_abort ();




  if (cvt <signed char, std::float16_t> (42.0f16) != (signed char) (std::float16_t) 42.0f16
      || cvt <signed char, std::float16_t> (-42.0f16) != (signed char) (std::float16_t) -42.0f16

      || cvt <signed char, std::float16_t> ((signed char) 1 << (8 * sizeof (signed char) - 2)) != (signed char) (std::float16_t) ((signed char) 1 << (8 * sizeof (signed char) - 2))
      || cvt <signed char, std::float16_t> (-((signed char) 1 << (8 * sizeof (signed char) - 2))) != (signed char) (std::float16_t) (-((signed char) 1 << (8 * sizeof (signed char) - 2)))

     )
    __builtin_abort ();
  if (cvt <unsigned char, std::float16_t> (42.0f16) != (unsigned char) (std::float16_t) 42.0f16

      || cvt <unsigned char, std::float16_t> ((unsigned char) 1 << (8 * sizeof (unsigned char) - 1)) != (unsigned char) (std::float16_t) ((unsigned char) 1 << (8 * sizeof (unsigned char) - 1))

     )
    __builtin_abort ();
  if (cvt <signed short, std::float16_t> (42.0f16) != (signed short) (std::float16_t) 42.0f16
      || cvt <signed short, std::float16_t> (-42.0f16) != (signed short) (std::float16_t) -42.0f16

      || cvt <signed short, std::float16_t> ((signed short) 1 << (8 * sizeof (signed short) - 2)) != (signed short) (std::float16_t) ((signed short) 1 << (8 * sizeof (signed short) - 2))
      || cvt <signed short, std::float16_t> (-((signed short) 1 << (8 * sizeof (signed short) - 2))) != (signed short) (std::float16_t) (-((signed short) 1 << (8 * sizeof (signed short) - 2)))




     )
    __builtin_abort ();
  if (cvt <unsigned short, std::float16_t> (42.0f16) != (unsigned short) (std::float16_t) 42.0f16
      || cvt <unsigned short, std::float16_t> (65504.0f16) != (unsigned short) (std::float16_t) 65504.0f16)
    __builtin_abort ();
  if (cvt <signed int, std::float16_t> (42.0f16) != (signed int) (std::float16_t) 42.0f16
      || cvt <signed int, std::float16_t> (-42.0f16) != (signed int) (std::float16_t) -42.0f16




      || cvt <signed int, std::float16_t> (65504.0f16) != (signed int) (std::float16_t) 65504.0f16
      || cvt <signed int, std::float16_t> (-65504.0f16) != (signed int) (std::float16_t) -65504.0f16

     )
    __builtin_abort ();
  if (cvt <unsigned int, std::float16_t> (42.0f16) != (unsigned int) (std::float16_t) 42.0f16
      || cvt <unsigned int, std::float16_t> (65504.0f16) != (unsigned int) (std::float16_t) 65504.0f16)
    __builtin_abort ();
  if (cvt <signed long int, std::float16_t> (42.0f16) != (signed long int) (std::float16_t) 42.0f16
      || cvt <signed long int, std::float16_t> (-42.0f16) != (signed long int) (std::float16_t) -42.0f16
      || cvt <signed long int, std::float16_t> (65504.0f16) != (signed long int) (std::float16_t) 65504.0f16
      || cvt <signed long int, std::float16_t> (-65504.0f16) != (signed long int) (std::float16_t) -65504.0f16)
    __builtin_abort ();
  if (cvt <unsigned long int, std::float16_t> (42.0f16) != (unsigned long int) (std::float16_t) 42.0f16
      || cvt <unsigned long int, std::float16_t> (65504.0f16) != (unsigned long int) (std::float16_t) 65504.0f16)
    __builtin_abort ();
  if (cvt <signed long long int, std::float16_t> (42.0f16) != (signed long long int) (std::float16_t) 42.0f16
      || cvt <signed long long int, std::float16_t> (-42.0f16) != (signed long long int) (std::float16_t) -42.0f16
      || cvt <signed long long int, std::float16_t> (65504.0f16) != (signed long long int) (std::float16_t) 65504.0f16
      || cvt <signed long long int, std::float16_t> (-65504.0f16) != (signed long long int) (std::float16_t) -65504.0f16)
    __builtin_abort ();
  if (cvt <unsigned long long int, std::float16_t> (42.0f16) != (unsigned long long int) (std::float16_t) 42.0f16
      || cvt <unsigned long long int, std::float16_t> (65504.0f16) != (unsigned long long int) (std::float16_t) 65504.0f16)
    __builtin_abort ();

  if (cvt <signed __int128, std::float16_t> (42.0f16) != (signed __int128) (std::float16_t) 42.0f16
      || cvt <signed __int128, std::float16_t> (-42.0f16) != (signed __int128) (std::float16_t) -42.0f16
      || cvt <signed __int128, std::float16_t> (65504.0f16) != (signed __int128) (std::float16_t) 65504.0f16
      || cvt <signed __int128, std::float16_t> (-65504.0f16) != (signed __int128) (std::float16_t) -65504.0f16)
    __builtin_abort ();
  if (cvt <unsigned __int128, std::float16_t> (42.0f16) != (unsigned __int128) (std::float16_t) 42.0f16
      || cvt <unsigned __int128, std::float16_t> (65504.0f16) != (unsigned __int128) (std::float16_t) 65504.0f16)
    __builtin_abort ();



  if (cvt <signed char, std::bfloat16_t> (42.0bf16) != (signed char) (std::bfloat16_t) 42.0bf16
      || cvt <signed char, std::bfloat16_t> (-42.0bf16) != (signed char) (std::bfloat16_t) -42.0bf16
      || cvt <signed char, std::bfloat16_t> ((signed char) 1 << (8 * sizeof (signed char) - 2)) != (signed char) (std::bfloat16_t) ((signed char) 1 << (8 * sizeof (signed char) - 2))
      || cvt <signed char, std::bfloat16_t> (-((signed char) 1 << (8 * sizeof (signed char) - 2))) != (signed char) (std::bfloat16_t) (-((signed char) 1 << (8 * sizeof (signed char) - 2))))
    __builtin_abort ();
  if (cvt <unsigned char, std::bfloat16_t> (42.0bf16) != (unsigned char) (std::bfloat16_t) 42.0bf16
      || cvt <unsigned char, std::bfloat16_t> ((unsigned char) 1 << (8 * sizeof (unsigned char) - 1)) != (unsigned char) (std::bfloat16_t) ((unsigned char) 1 << (8 * sizeof (unsigned char) - 1)))
    __builtin_abort ();
  if (cvt <signed short, std::bfloat16_t> (42.0bf16) != (signed short) (std::bfloat16_t) 42.0bf16
      || cvt <signed short, std::bfloat16_t> (-42.0bf16) != (signed short) (std::bfloat16_t) -42.0bf16
      || cvt <signed short, std::bfloat16_t> ((signed short) 1 << (8 * sizeof (signed short) - 2)) != (signed short) (std::bfloat16_t) ((signed short) 1 << (8 * sizeof (signed short) - 2))
      || cvt <signed short, std::bfloat16_t> (-((signed short) 1 << (8 * sizeof (signed short) - 2))) != (signed short) (std::bfloat16_t) (-((signed short) 1 << (8 * sizeof (signed short) - 2))))
    __builtin_abort ();
  if (cvt <unsigned short, std::bfloat16_t> (42.0bf16) != (unsigned short) (std::bfloat16_t) 42.0bf16
      || cvt <unsigned short, std::bfloat16_t> ((unsigned short) 1 << (8 * sizeof (unsigned short) - 1)) != (unsigned short) (std::bfloat16_t) ((unsigned short) 1 << (8 * sizeof (unsigned short) - 1)))
    __builtin_abort ();
  if (cvt <signed int, std::bfloat16_t> (42.0bf16) != (signed int) (std::bfloat16_t) 42.0bf16
      || cvt <signed int, std::bfloat16_t> (-42.0bf16) != (signed int) (std::bfloat16_t) -42.0bf16
      || cvt <signed int, std::bfloat16_t> ((signed int) 1 << (8 * sizeof (signed int) - 2)) != (signed int) (std::bfloat16_t) ((signed int) 1 << (8 * sizeof (signed int) - 2))
      || cvt <signed int, std::bfloat16_t> (-((signed int) 1 << (8 * sizeof (signed int) - 2))) != (signed int) (std::bfloat16_t) (-((signed int) 1 << (8 * sizeof (signed int) - 2))))
    __builtin_abort ();
  if (cvt <unsigned int, std::bfloat16_t> (42.0bf16) != (unsigned int) (std::bfloat16_t) 42.0bf16
      || cvt <unsigned int, std::bfloat16_t> ((unsigned int) 1 << (8 * sizeof (unsigned int) - 1)) != (unsigned int) (std::bfloat16_t) ((unsigned int) 1 << (8 * sizeof (unsigned int) - 1)))
    __builtin_abort ();
  if (cvt <signed long int, std::bfloat16_t> (42.0bf16) != (signed long int) (std::bfloat16_t) 42.0bf16
      || cvt <signed long int, std::bfloat16_t> (-42.0bf16) != (signed long int) (std::bfloat16_t) -42.0bf16
      || cvt <signed long int, std::bfloat16_t> ((signed long int) 1 << (8 * sizeof (signed long int) - 2)) != (signed long int) (std::bfloat16_t) ((signed long int) 1 << (8 * sizeof (signed long int) - 2))
      || cvt <signed long int, std::bfloat16_t> (-((signed long int) 1 << (8 * sizeof (signed long int) - 2))) != (signed long int) (std::bfloat16_t) (-((signed long int) 1 << (8 * sizeof (signed long int) - 2))))
    __builtin_abort ();
  if (cvt <unsigned long int, std::bfloat16_t> (42.0bf16) != (unsigned long int) (std::bfloat16_t) 42.0bf16
      || cvt <unsigned long int, std::bfloat16_t> ((unsigned long int) 1 << (8 * sizeof (unsigned long int) - 1)) != (unsigned long int) (std::bfloat16_t) ((unsigned long int) 1 << (8 * sizeof (unsigned long int) - 1)))
    __builtin_abort ();
  if (cvt <signed long long int, std::bfloat16_t> (42.0bf16) != (signed long long int) (std::bfloat16_t) 42.0bf16
      || cvt <signed long long int, std::bfloat16_t> (-42.0bf16) != (signed long long int) (std::bfloat16_t) -42.0bf16
      || cvt <signed long long int, std::bfloat16_t> ((signed long long int) 1 << (8 * sizeof (signed long long int) - 2)) != (signed long long int) (std::bfloat16_t) ((signed long long int) 1 << (8 * sizeof (signed long long int) - 2))
      || cvt <signed long long int, std::bfloat16_t> (-((signed long long int) 1 << (8 * sizeof (signed long long int) - 2))) != (signed long long int) (std::bfloat16_t) (-((signed long long int) 1 << (8 * sizeof (signed long long int) - 2))))
    __builtin_abort ();
  if (cvt <unsigned long long int, std::bfloat16_t> (42.0bf16) != (unsigned long long int) (std::bfloat16_t) 42.0bf16
      || cvt <unsigned long long int, std::bfloat16_t> ((unsigned long long int) 1 << (8 * sizeof (unsigned long long int) - 1)) != (unsigned long long int) (std::bfloat16_t) ((unsigned long long int) 1 << (8 * sizeof (unsigned long long int) - 1)))
    __builtin_abort ();

  if (cvt <signed __int128, std::bfloat16_t> (42.0bf16) != (signed __int128) (std::bfloat16_t) 42.0bf16
      || cvt <signed __int128, std::bfloat16_t> (-42.0bf16) != (signed __int128) (std::bfloat16_t) -42.0bf16
      || cvt <signed __int128, std::bfloat16_t> ((signed __int128) 1 << (8 * sizeof (signed __int128) - 2)) != (signed __int128) (std::bfloat16_t) ((signed __int128) 1 << (8 * sizeof (signed __int128) - 2))
      || cvt <signed __int128, std::bfloat16_t> (-((signed __int128) 1 << (8 * sizeof (signed __int128) - 2))) != (signed __int128) (std::bfloat16_t) (-((signed __int128) 1 << (8 * sizeof (signed __int128) - 2))))
    __builtin_abort ();
  if (cvt <unsigned __int128, std::bfloat16_t> (42.0bf16) != (unsigned __int128) (std::bfloat16_t) 42.0bf16
      || cvt <unsigned __int128, std::bfloat16_t> ((unsigned __int128) 1 << (8 * sizeof (unsigned __int128) - 1)) != (unsigned __int128) (std::bfloat16_t) ((unsigned __int128) 1 << (8 * sizeof (unsigned __int128) - 1)))
    __builtin_abort ();



  if (cvt <signed char, std::float32_t> (42.0f32) != (signed char) (std::float32_t) 42.0f32
      || cvt <signed char, std::float32_t> (-42.0f32) != (signed char) (std::float32_t) -42.0f32
      || cvt <signed char, std::float32_t> ((signed char) 1 << (8 * sizeof (signed char) - 2)) != (signed char) (std::float32_t) ((signed char) 1 << (8 * sizeof (signed char) - 2))
      || cvt <signed char, std::float32_t> (-((signed char) 1 << (8 * sizeof (signed char) - 2))) != (signed char) (std::float32_t) (-((signed char) 1 << (8 * sizeof (signed char) - 2))))
    __builtin_abort ();
  if (cvt <unsigned char, std::float32_t> (42.0f32) != (unsigned char) (std::float32_t) 42.0f32
      || cvt <unsigned char, std::float32_t> ((unsigned char) 1 << (8 * sizeof (unsigned char) - 1)) != (unsigned char) (std::float32_t) ((unsigned char) 1 << (8 * sizeof (unsigned char) - 1)))
    __builtin_abort ();
  if (cvt <signed short, std::float32_t> (42.0f32) != (signed short) (std::float32_t) 42.0f32
      || cvt <signed short, std::float32_t> (-42.0f32) != (signed short) (std::float32_t) -42.0f32
      || cvt <signed short, std::float32_t> ((signed short) 1 << (8 * sizeof (signed short) - 2)) != (signed short) (std::float32_t) ((signed short) 1 << (8 * sizeof (signed short) - 2))
      || cvt <signed short, std::float32_t> (-((signed short) 1 << (8 * sizeof (signed short) - 2))) != (signed short) (std::float32_t) (-((signed short) 1 << (8 * sizeof (signed short) - 2))))
    __builtin_abort ();
  if (cvt <unsigned short, std::float32_t> (42.0f32) != (unsigned short) (std::float32_t) 42.0f32
      || cvt <unsigned short, std::float32_t> ((unsigned short) 1 << (8 * sizeof (unsigned short) - 1)) != (unsigned short) (std::float32_t) ((unsigned short) 1 << (8 * sizeof (unsigned short) - 1)))
    __builtin_abort ();
  if (cvt <signed int, std::float32_t> (42.0f32) != (signed int) (std::float32_t) 42.0f32
      || cvt <signed int, std::float32_t> (-42.0f32) != (signed int) (std::float32_t) -42.0f32
      || cvt <signed int, std::float32_t> ((signed int) 1 << (8 * sizeof (signed int) - 2)) != (signed int) (std::float32_t) ((signed int) 1 << (8 * sizeof (signed int) - 2))
      || cvt <signed int, std::float32_t> (-((signed int) 1 << (8 * sizeof (signed int) - 2))) != (signed int) (std::float32_t) (-((signed int) 1 << (8 * sizeof (signed int) - 2))))
    __builtin_abort ();
  if (cvt <unsigned int, std::float32_t> (42.0f32) != (unsigned int) (std::float32_t) 42.0f32
      || cvt <unsigned int, std::float32_t> ((unsigned int) 1 << (8 * sizeof (unsigned int) - 1)) != (unsigned int) (std::float32_t) ((unsigned int) 1 << (8 * sizeof (unsigned int) - 1)))
    __builtin_abort ();
  if (cvt <signed long int, std::float32_t> (42.0f32) != (signed long int) (std::float32_t) 42.0f32
      || cvt <signed long int, std::float32_t> (-42.0f32) != (signed long int) (std::float32_t) -42.0f32
      || cvt <signed long int, std::float32_t> ((signed long int) 1 << (8 * sizeof (signed long int) - 2)) != (signed long int) (std::float32_t) ((signed long int) 1 << (8 * sizeof (signed long int) - 2))
      || cvt <signed long int, std::float32_t> (-((signed long int) 1 << (8 * sizeof (signed long int) - 2))) != (signed long int) (std::float32_t) (-((signed long int) 1 << (8 * sizeof (signed long int) - 2))))
    __builtin_abort ();
  if (cvt <unsigned long int, std::float32_t> (42.0f32) != (unsigned long int) (std::float32_t) 42.0f32
      || cvt <unsigned long int, std::float32_t> ((unsigned long int) 1 << (8 * sizeof (unsigned long int) - 1)) != (unsigned long int) (std::float32_t) ((unsigned long int) 1 << (8 * sizeof (unsigned long int) - 1)))
    __builtin_abort ();
  if (cvt <signed long long int, std::float32_t> (42.0f32) != (signed long long int) (std::float32_t) 42.0f32
      || cvt <signed long long int, std::float32_t> (-42.0f32) != (signed long long int) (std::float32_t) -42.0f32
      || cvt <signed long long int, std::float32_t> ((signed long long int) 1 << (8 * sizeof (signed long long int) - 2)) != (signed long long int) (std::float32_t) ((signed long long int) 1 << (8 * sizeof (signed long long int) - 2))
      || cvt <signed long long int, std::float32_t> (-((signed long long int) 1 << (8 * sizeof (signed long long int) - 2))) != (signed long long int) (std::float32_t) (-((signed long long int) 1 << (8 * sizeof (signed long long int) - 2))))
    __builtin_abort ();
  if (cvt <unsigned long long int, std::float32_t> (42.0f32) != (unsigned long long int) (std::float32_t) 42.0f32
      || cvt <unsigned long long int, std::float32_t> ((unsigned long long int) 1 << (8 * sizeof (unsigned long long int) - 1)) != (unsigned long long int) (std::float32_t) ((unsigned long long int) 1 << (8 * sizeof (unsigned long long int) - 1)))
    __builtin_abort ();

  if (cvt <signed __int128, std::float32_t> (42.0f32) != (signed __int128) (std::float32_t) 42.0f32
      || cvt <signed __int128, std::float32_t> (-42.0f32) != (signed __int128) (std::float32_t) -42.0f32
      || cvt <signed __int128, std::float32_t> ((signed __int128) 1 << (8 * sizeof (signed __int128) - 2)) != (signed __int128) (std::float32_t) ((signed __int128) 1 << (8 * sizeof (signed __int128) - 2))
      || cvt <signed __int128, std::float32_t> (-((signed __int128) 1 << (8 * sizeof (signed __int128) - 2))) != (signed __int128) (std::float32_t) (-((signed __int128) 1 << (8 * sizeof (signed __int128) - 2))))
    __builtin_abort ();
  if (cvt <unsigned __int128, std::float32_t> (42.0f32) != (unsigned __int128) (std::float32_t) 42.0f32
      || cvt <unsigned __int128, std::float32_t> ((unsigned __int128) 1 << (8 * sizeof (unsigned __int128) - 1)) != (unsigned __int128) (std::float32_t) ((unsigned __int128) 1 << (8 * sizeof (unsigned __int128) - 1)))
    __builtin_abort ();



  if (cvt <signed char, std::float64_t> (42.0f64) != (signed char) (std::float64_t) 42.0f64
      || cvt <signed char, std::float64_t> (-42.0f64) != (signed char) (std::float64_t) -42.0f64
      || cvt <signed char, std::float64_t> ((signed char) 1 << (8 * sizeof (signed char) - 2)) != (signed char) (std::float64_t) ((signed char) 1 << (8 * sizeof (signed char) - 2))
      || cvt <signed char, std::float64_t> (-((signed char) 1 << (8 * sizeof (signed char) - 2))) != (signed char) (std::float64_t) (-((signed char) 1 << (8 * sizeof (signed char) - 2))))
    __builtin_abort ();
  if (cvt <unsigned char, std::float64_t> (42.0f64) != (unsigned char) (std::float64_t) 42.0f64
      || cvt <unsigned char, std::float64_t> ((unsigned char) 1 << (8 * sizeof (unsigned char) - 1)) != (unsigned char) (std::float64_t) ((unsigned char) 1 << (8 * sizeof (unsigned char) - 1)))
    __builtin_abort ();
  if (cvt <signed short, std::float64_t> (42.0f64) != (signed short) (std::float64_t) 42.0f64
      || cvt <signed short, std::float64_t> (-42.0f64) != (signed short) (std::float64_t) -42.0f64
      || cvt <signed short, std::float64_t> ((signed short) 1 << (8 * sizeof (signed short) - 2)) != (signed short) (std::float64_t) ((signed short) 1 << (8 * sizeof (signed short) - 2))
      || cvt <signed short, std::float64_t> (-((signed short) 1 << (8 * sizeof (signed short) - 2))) != (signed short) (std::float64_t) (-((signed short) 1 << (8 * sizeof (signed short) - 2))))
    __builtin_abort ();
  if (cvt <unsigned short, std::float64_t> (42.0f64) != (unsigned short) (std::float64_t) 42.0f64
      || cvt <unsigned short, std::float64_t> ((unsigned short) 1 << (8 * sizeof (unsigned short) - 1)) != (unsigned short) (std::float64_t) ((unsigned short) 1 << (8 * sizeof (unsigned short) - 1)))
    __builtin_abort ();
  if (cvt <signed int, std::float64_t> (42.0f64) != (signed int) (std::float64_t) 42.0f64
      || cvt <signed int, std::float64_t> (-42.0f64) != (signed int) (std::float64_t) -42.0f64
      || cvt <signed int, std::float64_t> ((signed int) 1 << (8 * sizeof (signed int) - 2)) != (signed int) (std::float64_t) ((signed int) 1 << (8 * sizeof (signed int) - 2))
      || cvt <signed int, std::float64_t> (-((signed int) 1 << (8 * sizeof (signed int) - 2))) != (signed int) (std::float64_t) (-((signed int) 1 << (8 * sizeof (signed int) - 2))))
    __builtin_abort ();
  if (cvt <unsigned int, std::float64_t> (42.0f64) != (unsigned int) (std::float64_t) 42.0f64
      || cvt <unsigned int, std::float64_t> ((unsigned int) 1 << (8 * sizeof (unsigned int) - 1)) != (unsigned int) (std::float64_t) ((unsigned int) 1 << (8 * sizeof (unsigned int) - 1)))
    __builtin_abort ();
  if (cvt <signed long int, std::float64_t> (42.0f64) != (signed long int) (std::float64_t) 42.0f64
      || cvt <signed long int, std::float64_t> (-42.0f64) != (signed long int) (std::float64_t) -42.0f64
      || cvt <signed long int, std::float64_t> ((signed long int) 1 << (8 * sizeof (signed long int) - 2)) != (signed long int) (std::float64_t) ((signed long int) 1 << (8 * sizeof (signed long int) - 2))
      || cvt <signed long int, std::float64_t> (-((signed long int) 1 << (8 * sizeof (signed long int) - 2))) != (signed long int) (std::float64_t) (-((signed long int) 1 << (8 * sizeof (signed long int) - 2))))
    __builtin_abort ();
  if (cvt <unsigned long int, std::float64_t> (42.0f64) != (unsigned long int) (std::float64_t) 42.0f64
      || cvt <unsigned long int, std::float64_t> ((unsigned long int) 1 << (8 * sizeof (unsigned long int) - 1)) != (unsigned long int) (std::float64_t) ((unsigned long int) 1 << (8 * sizeof (unsigned long int) - 1)))
    __builtin_abort ();
  if (cvt <signed long long int, std::float64_t> (42.0f64) != (signed long long int) (std::float64_t) 42.0f64
      || cvt <signed long long int, std::float64_t> (-42.0f64) != (signed long long int) (std::float64_t) -42.0f64
      || cvt <signed long long int, std::float64_t> ((signed long long int) 1 << (8 * sizeof (signed long long int) - 2)) != (signed long long int) (std::float64_t) ((signed long long int) 1 << (8 * sizeof (signed long long int) - 2))
      || cvt <signed long long int, std::float64_t> (-((signed long long int) 1 << (8 * sizeof (signed long long int) - 2))) != (signed long long int) (std::float64_t) (-((signed long long int) 1 << (8 * sizeof (signed long long int) - 2))))
    __builtin_abort ();
  if (cvt <unsigned long long int, std::float64_t> (42.0f64) != (unsigned long long int) (std::float64_t) 42.0f64
      || cvt <unsigned long long int, std::float64_t> ((unsigned long long int) 1 << (8 * sizeof (unsigned long long int) - 1)) != (unsigned long long int) (std::float64_t) ((unsigned long long int) 1 << (8 * sizeof (unsigned long long int) - 1)))
    __builtin_abort ();

  if (cvt <signed __int128, std::float64_t> (42.0f64) != (signed __int128) (std::float64_t) 42.0f64
      || cvt <signed __int128, std::float64_t> (-42.0f64) != (signed __int128) (std::float64_t) -42.0f64
      || cvt <signed __int128, std::float64_t> ((signed __int128) 1 << (8 * sizeof (signed __int128) - 2)) != (signed __int128) (std::float64_t) ((signed __int128) 1 << (8 * sizeof (signed __int128) - 2))
      || cvt <signed __int128, std::float64_t> (-((signed __int128) 1 << (8 * sizeof (signed __int128) - 2))) != (signed __int128) (std::float64_t) (-((signed __int128) 1 << (8 * sizeof (signed __int128) - 2))))
    __builtin_abort ();
  if (cvt <unsigned __int128, std::float64_t> (42.0f64) != (unsigned __int128) (std::float64_t) 42.0f64
      || cvt <unsigned __int128, std::float64_t> ((unsigned __int128) 1 << (8 * sizeof (unsigned __int128) - 1)) != (unsigned __int128) (std::float64_t) ((unsigned __int128) 1 << (8 * sizeof (unsigned __int128) - 1)))
    __builtin_abort ();



  if (cvt <signed char, std::float128_t> (42.0f128) != (signed char) (std::float128_t) 42.0f128
      || cvt <signed char, std::float128_t> (-42.0f128) != (signed char) (std::float128_t) -42.0f128
      || cvt <signed char, std::float128_t> ((signed char) 1 << (8 * sizeof (signed char) - 2)) != (signed char) (std::float128_t) ((signed char) 1 << (8 * sizeof (signed char) - 2))
      || cvt <signed char, std::float128_t> (-((signed char) 1 << (8 * sizeof (signed char) - 2))) != (signed char) (std::float128_t) (-((signed char) 1 << (8 * sizeof (signed char) - 2))))
    __builtin_abort ();
  if (cvt <unsigned char, std::float128_t> (42.0f128) != (unsigned char) (std::float128_t) 42.0f128
      || cvt <unsigned char, std::float128_t> ((unsigned char) 1 << (8 * sizeof (unsigned char) - 1)) != (unsigned char) (std::float128_t) ((unsigned char) 1 << (8 * sizeof (unsigned char) - 1)))
    __builtin_abort ();
  if (cvt <signed short, std::float128_t> (42.0f128) != (signed short) (std::float128_t) 42.0f128
      || cvt <signed short, std::float128_t> (-42.0f128) != (signed short) (std::float128_t) -42.0f128
      || cvt <signed short, std::float128_t> ((signed short) 1 << (8 * sizeof (signed short) - 2)) != (signed short) (std::float128_t) ((signed short) 1 << (8 * sizeof (signed short) - 2))
      || cvt <signed short, std::float128_t> (-((signed short) 1 << (8 * sizeof (signed short) - 2))) != (signed short) (std::float128_t) (-((signed short) 1 << (8 * sizeof (signed short) - 2))))
    __builtin_abort ();
  if (cvt <unsigned short, std::float128_t> (42.0f128) != (unsigned short) (std::float128_t) 42.0f128
      || cvt <unsigned short, std::float128_t> ((unsigned short) 1 << (8 * sizeof (unsigned short) - 1)) != (unsigned short) (std::float128_t) ((unsigned short) 1 << (8 * sizeof (unsigned short) - 1)))
    __builtin_abort ();
  if (cvt <signed int, std::float128_t> (42.0f128) != (signed int) (std::float128_t) 42.0f128
      || cvt <signed int, std::float128_t> (-42.0f128) != (signed int) (std::float128_t) -42.0f128
      || cvt <signed int, std::float128_t> ((signed int) 1 << (8 * sizeof (signed int) - 2)) != (signed int) (std::float128_t) ((signed int) 1 << (8 * sizeof (signed int) - 2))
      || cvt <signed int, std::float128_t> (-((signed int) 1 << (8 * sizeof (signed int) - 2))) != (signed int) (std::float128_t) (-((signed int) 1 << (8 * sizeof (signed int) - 2))))
    __builtin_abort ();
  if (cvt <unsigned int, std::float128_t> (42.0f128) != (unsigned int) (std::float128_t) 42.0f128
      || cvt <unsigned int, std::float128_t> ((unsigned int) 1 << (8 * sizeof (unsigned int) - 1)) != (unsigned int) (std::float128_t) ((unsigned int) 1 << (8 * sizeof (unsigned int) - 1)))
    __builtin_abort ();
  if (cvt <signed long int, std::float128_t> (42.0f128) != (signed long int) (std::float128_t) 42.0f128
      || cvt <signed long int, std::float128_t> (-42.0f128) != (signed long int) (std::float128_t) -42.0f128
      || cvt <signed long int, std::float128_t> ((signed long int) 1 << (8 * sizeof (signed long int) - 2)) != (signed long int) (std::float128_t) ((signed long int) 1 << (8 * sizeof (signed long int) - 2))
      || cvt <signed long int, std::float128_t> (-((signed long int) 1 << (8 * sizeof (signed long int) - 2))) != (signed long int) (std::float128_t) (-((signed long int) 1 << (8 * sizeof (signed long int) - 2))))
    __builtin_abort ();
  if (cvt <unsigned long int, std::float128_t> (42.0f128) != (unsigned long int) (std::float128_t) 42.0f128
      || cvt <unsigned long int, std::float128_t> ((unsigned long int) 1 << (8 * sizeof (unsigned long int) - 1)) != (unsigned long int) (std::float128_t) ((unsigned long int) 1 << (8 * sizeof (unsigned long int) - 1)))
    __builtin_abort ();
  if (cvt <signed long long int, std::float128_t> (42.0f128) != (signed long long int) (std::float128_t) 42.0f128
      || cvt <signed long long int, std::float128_t> (-42.0f128) != (signed long long int) (std::float128_t) -42.0f128
      || cvt <signed long long int, std::float128_t> ((signed long long int) 1 << (8 * sizeof (signed long long int) - 2)) != (signed long long int) (std::float128_t) ((signed long long int) 1 << (8 * sizeof (signed long long int) - 2))
      || cvt <signed long long int, std::float128_t> (-((signed long long int) 1 << (8 * sizeof (signed long long int) - 2))) != (signed long long int) (std::float128_t) (-((signed long long int) 1 << (8 * sizeof (signed long long int) - 2))))
    __builtin_abort ();
  if (cvt <unsigned long long int, std::float128_t> (42.0f128) != (unsigned long long int) (std::float128_t) 42.0f128
      || cvt <unsigned long long int, std::float128_t> ((unsigned long long int) 1 << (8 * sizeof (unsigned long long int) - 1)) != (unsigned long long int) (std::float128_t) ((unsigned long long int) 1 << (8 * sizeof (unsigned long long int) - 1)))
    __builtin_abort ();

  if (cvt <signed __int128, std::float128_t> (42.0f128) != (signed __int128) (std::float128_t) 42.0f128
      || cvt <signed __int128, std::float128_t> (-42.0f128) != (signed __int128) (std::float128_t) -42.0f128
      || cvt <signed __int128, std::float128_t> ((signed __int128) 1 << (8 * sizeof (signed __int128) - 2)) != (signed __int128) (std::float128_t) ((signed __int128) 1 << (8 * sizeof (signed __int128) - 2))
      || cvt <signed __int128, std::float128_t> (-((signed __int128) 1 << (8 * sizeof (signed __int128) - 2))) != (signed __int128) (std::float128_t) (-((signed __int128) 1 << (8 * sizeof (signed __int128) - 2))))
    __builtin_abort ();
  if (cvt <unsigned __int128, std::float128_t> (42.0f128) != (unsigned __int128) (std::float128_t) 42.0f128
      || cvt <unsigned __int128, std::float128_t> ((unsigned __int128) 1 << (8 * sizeof (unsigned __int128) - 1)) != (unsigned __int128) (std::float128_t) ((unsigned __int128) 1 << (8 * sizeof (unsigned __int128) - 1)))
    __builtin_abort ();




}

//type:fp
//options:--gn 40503:--gn 40604:--gn 80400:--gn 90100:--gn 120100:--gn 130200:--gn 130200 --target linux_aarch64:--gn 130200 --target linux_armv7:--clang_version 180100:--clang_version 180100 --target linux_aarch64:--clang_version 180100 --target linux_armv7:--clang_version 50000
//options_all:--c++11 -w

#if defined(__aarch64__) || defined(__arm__)
using float128 = long double;
using complex128 = long double __complex__;
#elif __GNUC__ >= 13
using float128 = _Float128;
using complex128 = _Float128 __complex__;
#else
using float128 = __float128;
using complex128 = __float128 __complex__;
#endif

#if defined(__GNUC__) && !defined(__clang__) && \
    (defined(__x86_64__) || defined(__i386__))
void f(float128 f)
{
  complex128 v1 = __divtc3(f, f, f, f);
  complex128 v2 = __multc3(f, f, f, f);

  complex128 (*p) (float128, float128, float128, float128) = &__divtc3;
}
#endif

#if defined(__aarch64__)
#if defined(__clang__) && __clang_major__ >= 11
void f(__SVInt8_t i)
{
  __SVFloat32_t v1 = __builtin_sve_reinterpret_f32_s8(i);

#if defined(__clang__) && __clang_major__ >= 17
  __SVCount_t v2 = __builtin_sve_svptrue_c64();
#endif
}
#endif

#if defined(__GNUC__) && !defined(__clang__)
void f(long i)
{
  long v = __builtin_aarch64_absdi(i);
  long (*p) (long) = &__builtin_aarch64_absdi;
}
#endif
#endif

inline namespace ns
{
  void __builtin_ia32_vfmsubpd();
  void __builtin_ia32_blendpd();
  void __builtin_ia32_addcarryx_u64();
}

template<typename T, typename First, typename ... Rest>
constexpr bool fn_has_params(T (*)(First, Rest ...))
{ return true; }

template<typename T>
constexpr bool fn_has_params(T *)
{ return false; }


// only available in gcc (4.5 and 9.x+) and clang 3.6 - 5.0
#if (defined(__x86_64__) || defined(__i386__)) && \
    ((defined(__clang__) && (__clang_major__ < 6)) || \
     (defined(__GNUC__) && \
      (__GNUC__ >= 9 || \
       (__GNUC__ == 4 && __GNUC_MINOR__ == 5))))
constexpr bool has_ia32_vfmsubpd = true;
#else
constexpr bool has_ia32_vfmsubpd = false;
#endif

static_assert(fn_has_params(__builtin_ia32_vfmsubpd) == has_ia32_vfmsubpd,
  "Unexpected");

// not available for clang 3.7 - 6.x
#if (!defined(__x86_64__) && !defined(__i386__)) || \
    (defined(__clang__) && \
     (__clang_major__ >= 3 && __clang_major__ <= 6 && \
      (__clang_major__ != 3 || __clang_minor__ >= 7)))
constexpr bool has_ia32_blendpd = false;
#else
constexpr bool has_ia32_blendpd = true;
#endif

static_assert(fn_has_params(__builtin_ia32_blendpd) == has_ia32_blendpd,
  "Unexpected");



#if defined(__x86_64__) && \
    (defined(__clang__) || __GNUC__ >= 5 || \
     (__GNUC__ == 4 && __GNUC_MINOR__ >= 8))
constexpr bool has_ia32_addcarryx_u64 = true;
#else
constexpr bool has_ia32_addcarryx_u64 = false;
#endif

static_assert(fn_has_params(__builtin_ia32_addcarryx_u64) == has_ia32_addcarryx_u64,
  "Unexpected");

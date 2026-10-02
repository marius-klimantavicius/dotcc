//type:fp
//options:--gn 130200 --target linux_aarch64:--gn 130200 --target linux_armv7
//options_all:-w --c++17

namespace gcc_neon_types
{
#if _LP64
  void f(__builtin_aarch64_simd_bf v)
  {
    __bf16 &r = v;
  }

  void f(__builtin_aarch64_simd_sf v)
  {
    float &r = v;
  }

  void f(__builtin_aarch64_simd_df v)
  {
    double &r = v;
  }

  void f(__builtin_aarch64_simd_qi v)
  {
    signed char &r = v;
  }

  void f(__builtin_aarch64_simd_uqi v)
  {
    unsigned char &r = v;
  }

  void f(__builtin_aarch64_simd_hi v)
  {
    short &r = v;
  }

  void f(__builtin_aarch64_simd_uhi v)
  {
    unsigned short &r = v;
  }

  void f(__builtin_aarch64_simd_si v)
  {
    int &r = v;
  }

  void f(__builtin_aarch64_simd_usi v)
  {
    unsigned int &r = v;
  }

  void f(__builtin_aarch64_simd_di v)
  {
    long &r = v;
  }

  void f(__builtin_aarch64_simd_udi v)
  {
    unsigned long &r = v;
  }

  void g(__builtin_aarch64_simd_poly8 v)
  {
    unsigned char &r = v;
    __builtin_aarch64_simd_uqi &r2 = v;
  }

  void g(__builtin_aarch64_simd_poly16 v)
  {
    unsigned short &r = v;
    __builtin_aarch64_simd_uhi &r2 = v;
  }

  void g(__builtin_aarch64_simd_poly64 v)
  {
    unsigned long &r = v;
    __builtin_aarch64_simd_udi &r2 = v;
  }

  void g(__builtin_aarch64_simd_poly128 v)
  {
    unsigned __int128 &r = v;
  }

  void f(__builtin_aarch64_simd_ti v)
  {
    __int128 &r = v;
  }

  void f(__builtin_aarch64_simd_oi v)
  {
    static_assert(sizeof(v) == 32);
  }

  void f(__builtin_aarch64_simd_ci v)
  {
    static_assert(sizeof(v) == 48);
  }

  void f(__builtin_aarch64_simd_xi v)
  {
    static_assert(sizeof(v) == 64);
  }
#else
  void f(__builtin_neon_bf v)
  {
    __bf16 &r = v;
  }

  void f(__builtin_neon_sf v)
  {
    float &r = v;
  }

  void f(__builtin_neon_df v)
  {
    double &r = v;
  }

  void f(__builtin_neon_qi v)
  {
    signed char &r = v;
  }

  void f(__builtin_neon_uqi v)
  {
    unsigned char &r = v;
  }

  void f(__builtin_neon_hi v)
  {
    short &r = v;
  }

  void f(__builtin_neon_uhi v)
  {
    unsigned short &r = v;
  }

  void f(__builtin_neon_si v)
  {
    int &r = v;
  }

  void f(__builtin_neon_usi v)
  {
    unsigned int &r = v;
  }

  void f(__builtin_neon_di v)
  {
    long long &r = v;
  }

  void f(__builtin_neon_udi v)
  {
    unsigned long long &r = v;
  }

  void g(__builtin_neon_poly8 v)
  {
    static_assert(sizeof(v) == sizeof(char));
  }

  void g(__builtin_neon_poly16 v)
  {
    static_assert(sizeof(v) == sizeof(short));
  }

  void g(__builtin_neon_poly64 v)
  {
    static_assert(sizeof(v) == sizeof(long long));
  }

  void g(__builtin_neon_poly128 v)
  {
    static_assert(sizeof(v) == 2 * sizeof(long long));
  }

  void f(__builtin_neon_ti v)
  {
    static_assert(sizeof(v) == 16);
  }

  void f(__builtin_neon_uti v)
  {
    static_assert(sizeof(v) == 16);
  }

  void f(__builtin_neon_ei v)
  {
    static_assert(sizeof(v) == 24);
  }

  void f(__builtin_neon_oi v)
  {
    static_assert(sizeof(v) == 32);
  }

  void f(__builtin_neon_ci v)
  {
    static_assert(sizeof(v) == 48);
  }

  void f(__builtin_neon_xi v)
  {
    static_assert(sizeof(v) == 64);
  }
#endif
}

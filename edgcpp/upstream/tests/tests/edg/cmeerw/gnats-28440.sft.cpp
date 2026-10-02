//type:fn
//options:--target linux_aarch64:--target linux_armv7
//options_all:--c++20 --gn 150100

void f()
{
#if defined(__aarch64__)
  {
    __builtin_aarch64_simd_oi *p = 1;
  }

  {
    __Float32x2_t *p = 2;
  }

  {
    __Poly16x4_t *p = 1;
  }

#elif defined(__arm__)
  {
    __builtin_neon_oi *p = 1;
  }

  {
    __simd64_float32_t *p = 2;
  }

  {
    __simd64_poly16_t *p = 1;
  }
#endif
}

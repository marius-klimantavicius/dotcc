//type:fp
//options:--gn 130200:--clang_version 180100
//options_all:--target linux_aarch64 -w --c++17

template<typename, typename>
constexpr bool is_same = false;

template<typename T>
constexpr bool is_same<T, T> = true;

namespace arm_neon_types
{
#if !defined(__clang__)
  static_assert(is_same<__Poly8_t, unsigned char>);
  static_assert(is_same<__Poly16_t, unsigned short>);
  static_assert(is_same<__Poly64_t, unsigned long>);
  static_assert(is_same<__Poly128_t, unsigned __int128>);

  void f(__Bfloat16x4_t v)
  {
    decltype(v) v2;
  }
  void f(__Bfloat16x8_t v)
  {
    decltype(v) v2;
  }
  void f(__Float16x4_t v)
  {
    decltype(v) v2;
  }
  void f(__Float16x8_t v)
  {
    decltype(v) v2;
  }
  void f(__Float32x2_t v)
  {
    decltype(v) v2;
  }
  void f(__Float32x4_t v)
  {
    decltype(v) v2;
  }
  void f(__Float64x1_t v)
  {
    decltype(v) v2;
  }
  void f(__Float64x2_t v)
  {
    decltype(v) v2;
  }

  void f(__Int8x8_t v)
  {
    decltype(v) v2;
  }
  void f(__Int8x16_t v)
  {
    decltype(v) v2;
  }
  void f(__Int16x4_t v)
  {
    decltype(v) v2;
  }
  void f(__Int16x8_t v)
  {
    decltype(v) v2;
  }
  void f(__Int32x2_t v)
  {
    decltype(v) v2;
  }
  void f(__Int32x4_t v)
  {
    decltype(v) v2;
  }
  void f(__Int64x1_t v)
  {
    decltype(v) v2;
  }
  void f(__Int64x2_t v)
  {
    decltype(v) v2;
  }

  void f(__Uint8x8_t v)
  {
    decltype(v) v2;
  }
  void f(__Uint8x16_t v)
  {
    decltype(v) v2;
  }
  void f(__Poly8_t v)
  {
    decltype(v) v2;
  }
  void f(__Poly8x8_t v)
  {
    decltype(v) v2;
  }
  void f(__Poly8x16_t v)
  {
    decltype(v) v2;
  }
  void f(__Uint16x4_t v)
  {
    decltype(v) v2;
  }
  void f(__Uint16x8_t v)
  {
    decltype(v) v2;
  }
  void f(__Poly16_t v)
  {
    decltype(v) v2;
  }
  void f(__Poly16x8_t v)
  {
    decltype(v) v2;
  }
  void f(__Poly16x4_t v)
  {
    decltype(v) v2;
  }
  void f(__Uint32x2_t v)
  {
    decltype(v) v2;
  }
  void f(__Uint32x4_t v)
  {
    decltype(v) v2;
  }
  void f(__Uint64x1_t v)
  {
    decltype(v) v2;
  }
  void f(__Uint64x2_t v)
  {
    decltype(v) v2;
  }
  void f(__Poly64_t v)
  {
    decltype(v) v2;
  }
  void f(__Poly64x1_t v)
  {
    decltype(v) v2;
  }
  void f(__Poly64x2_t v)
  {
    decltype(v) v2;
  }
  void f(__Poly128_t v)
  {
    decltype(v) v2;
  }
#else
  void f(__attribute__((neon_vector_type(4))) __bf16 v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(8))) __bf16 v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(4))) __fp16 v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(8))) __fp16 v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(2))) float v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(4))) float v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(1))) double v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(2))) double v)
  {
    decltype(v) v2;
  }

  void f(__attribute__((neon_vector_type(8))) signed char v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(16))) signed char v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(4))) short v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(8))) short v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(2))) int v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(4))) int v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(1))) long v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(2))) long v)
  {
    decltype(v) v2;
  }

  void f(__attribute__((neon_vector_type(8))) unsigned char v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(16))) unsigned char v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_polyvector_type(8))) unsigned char v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_polyvector_type(16))) unsigned char v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(4))) unsigned short v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(8))) unsigned short v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_polyvector_type(4))) unsigned short v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_polyvector_type(8))) unsigned short v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(2))) unsigned int v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(4))) unsigned int v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(1))) unsigned long v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_vector_type(2))) unsigned long v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_polyvector_type(1))) unsigned long v)
  {
    decltype(v) v2;
  }
  void f(__attribute__((neon_polyvector_type(2))) unsigned long v)
  {
    decltype(v) v2;
  }
#endif
}

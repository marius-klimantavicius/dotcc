//type:fp
//options:--clang_version 200100:--gn 150100
//options_all:-w --c++17 --target linux_aarch64 --il_display
//filter:awk '/^func-scope variable@/{print $0; f=1; next}/^$/{if (f) print $0; f=0}f' | grep -E -e '^(  name|  enclosing_routine|type):' -e '^func-scope ' -e '^$' | sed -e 's/@[0-9a-f]*//'

#if defined(__clang__)
typedef __attribute__((neon_vector_type(8)))  __mfp8 mfloat8x8_t;
typedef __attribute__((neon_vector_type(16))) __mfp8 mfloat8x16_t;
#endif

namespace mfp8_types
{
  void f(__mfp8 v) { }
  void f(__SVMfloat8_t v) { }
#if defined(__clang__)
  void f(__clang_svmfloat8x2_t v) { }
  void f(__clang_svmfloat8x3_t v) { }
  void f(__clang_svmfloat8x4_t v) { }

  void f(mfloat8x8_t v) { }
  void f(mfloat8x16_t v) { }
#else
  void f(__Mfloat8x8_t v) { }
  void f(__Mfloat8x16_t v) { }
#endif
}

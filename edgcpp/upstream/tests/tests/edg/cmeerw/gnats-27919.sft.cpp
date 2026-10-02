//type:fp
//options:--clang_version 200100:--gn 150100
//options_all:-w --c++20 --target linux_aarch64

static_assert(sizeof(__mfp8) == 1);
static_assert(alignof(__mfp8) == 1);

void f(__mfp8 v)
{
  __mfp8 l;
  l = v;
  l = __mfp8();

#ifdef __EDG__
  __edg_vector_type__(__mfp8, 8)  lv8;
  __edg_vector_type__(__mfp8, 16) lv16;

  __edg_neon_vector_type__(__mfp8, 8)  nv8;
  __edg_neon_vector_type__(__mfp8, 16) nv16;
#endif
}

__mfp8 gm;

void f(__SVMfloat8_t v)
{
  __SVMfloat8_t l;
  l = v;
  l = __SVMfloat8_t();

#ifdef __EDG__
  __edg_scalable_vector_type__(__mfp8, 1) lv;
  lv = v;
#endif
}

#if defined(__clang__)
void f(__clang_svmfloat8x2_t v)
{
  __clang_svmfloat8x2_t l;
  l = v;
  l = __clang_svmfloat8x2_t();

#ifdef __EDG__
  __edg_scalable_vector_type__(__mfp8, 2) lv;
  lv = v;
#endif
}

void f(__clang_svmfloat8x3_t v)
{
  __clang_svmfloat8x3_t l;
  l = v;
  l = __clang_svmfloat8x3_t();

#ifdef __EDG__
  __edg_scalable_vector_type__(__mfp8, 3) lv;
  lv = v;
#endif
}

void f(__clang_svmfloat8x4_t v)
{
  __clang_svmfloat8x4_t l;
  l = v;
  l = __clang_svmfloat8x4_t();

#ifdef __EDG__
  __edg_scalable_vector_type__(__mfp8, 4) lv;
  lv = v;
#endif
}


typedef __attribute__((neon_vector_type(8)))  __mfp8 mfloat8x8_t;
typedef __attribute__((neon_vector_type(16))) __mfp8 mfloat8x16_t;

void f(mfloat8x8_t v8, mfloat8x16_t v16)
{
  __builtin_neon_vcvt1_bf16_mf8_fpm(v8, 0);
  __builtin_neon_vcvt1_high_bf16_mf8_fpm(v16, 0);
}
#else
void f(__Mfloat8x8_t v8, __Mfloat8x16_t v16)
{ }
#endif

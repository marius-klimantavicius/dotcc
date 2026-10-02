//type:fp
//options:--clang_version 100000:--clang_version 110000:--clang_version 180100:--gn 100100
//options_all:--target linux_aarch64 -w --c++17

namespace arm_sve_types
{
  typedef __SVInt8_t svint8_t;
  typedef __SVInt16_t svint16_t;
  typedef __SVInt32_t svint32_t;
  typedef __SVInt64_t svint64_t;
  typedef __SVUint8_t svuint8_t;
  typedef __SVUint16_t svuint16_t;
  typedef __SVUint32_t svuint32_t;
  typedef __SVUint64_t svuint64_t;
  typedef __SVFloat16_t svfloat16_t;
#if defined(__clang__) && __clang_major__ < 18
  typedef __SVBFloat16_t svbfloat16_t;
#else
  typedef __SVBfloat16_t svbfloat16_t;
#endif
  typedef __SVFloat32_t svfloat32_t;
  typedef __SVFloat64_t svfloat64_t;
#if __clang_major__ >= 11
  typedef __clang_svint8x2_t svint8x2_t;
  typedef __clang_svint16x2_t svint16x2_t;
  typedef __clang_svint32x2_t svint32x2_t;
  typedef __clang_svint64x2_t svint64x2_t;
  typedef __clang_svuint8x2_t svuint8x2_t;
  typedef __clang_svuint16x2_t svuint16x2_t;
  typedef __clang_svuint32x2_t svuint32x2_t;
  typedef __clang_svuint64x2_t svuint64x2_t;
  typedef __clang_svfloat16x2_t svfloat16x2_t;
  typedef __clang_svfloat32x2_t svfloat32x2_t;
  typedef __clang_svfloat64x2_t svfloat64x2_t;
  typedef __clang_svint8x3_t svint8x3_t;
  typedef __clang_svint16x3_t svint16x3_t;
  typedef __clang_svint32x3_t svint32x3_t;
  typedef __clang_svint64x3_t svint64x3_t;
  typedef __clang_svuint8x3_t svuint8x3_t;
  typedef __clang_svuint16x3_t svuint16x3_t;
  typedef __clang_svuint32x3_t svuint32x3_t;
  typedef __clang_svuint64x3_t svuint64x3_t;
  typedef __clang_svfloat16x3_t svfloat16x3_t;
  typedef __clang_svfloat32x3_t svfloat32x3_t;
  typedef __clang_svfloat64x3_t svfloat64x3_t;
  typedef __clang_svint8x4_t svint8x4_t;
  typedef __clang_svint16x4_t svint16x4_t;
  typedef __clang_svint32x4_t svint32x4_t;
  typedef __clang_svint64x4_t svint64x4_t;
  typedef __clang_svuint8x4_t svuint8x4_t;
  typedef __clang_svuint16x4_t svuint16x4_t;
  typedef __clang_svuint32x4_t svuint32x4_t;
  typedef __clang_svuint64x4_t svuint64x4_t;
  typedef __clang_svfloat16x4_t svfloat16x4_t;
  typedef __clang_svfloat32x4_t svfloat32x4_t;
  typedef __clang_svfloat64x4_t svfloat64x4_t;
  typedef __clang_svbfloat16x2_t svbfloat16x2_t;
  typedef __clang_svbfloat16x3_t svbfloat16x3_t;
  typedef __clang_svbfloat16x4_t svbfloat16x4_t;
#endif

  typedef __SVBool_t  svbool_t;
#if __clang_major__ >= 17
  typedef __clang_svboolx2_t  svboolx2_t;
  typedef __clang_svboolx4_t  svboolx4_t;
#endif

#if __clang_major__ >= 17
  typedef __SVCount_t svcount_t;
#endif
}

__SVInt8_t use_as_return_type();

void use_as_param_type(__SVInt8_t v)
{ }

void use_in_overload_set(__SVInt8_t);
void use_in_overload_set(int);;

__SVInt8_t return_value(__SVInt8_t v)
{
  return v;
}

#if __clang_major__ >= 17
__SVCount_t return_value(__SVCount_t v)
{
  return v;
}
#endif

void f()
{
  __SVInt8_t v;
  v = use_as_return_type();
  use_as_param_type(v);
  use_in_overload_set(v);
  use_in_overload_set(1);
}

static_assert(!__is_abstract(__SVInt8_t));
static_assert(!__is_class(__SVInt8_t));
static_assert(__is_convertible_to(__SVInt8_t, __SVInt8_t));
static_assert(!__is_empty(__SVInt8_t));
static_assert(!__is_enum(__SVInt8_t));
static_assert(!__is_function(__SVInt8_t));
static_assert(__is_pod(__SVInt8_t));
static_assert(!__is_polymorphic(__SVInt8_t));
static_assert(!__is_union(__SVInt8_t));
static_assert(__is_trivial(__SVInt8_t));
static_assert(!__is_standard_layout(__SVInt8_t));
static_assert(__is_trivially_copyable(__SVInt8_t));
static_assert(!__is_literal_type(__SVInt8_t));
static_assert(!__is_final(__SVInt8_t));
static_assert(!__has_unique_object_representations(__SVInt8_t));
static_assert(!__is_aggregate(__SVInt8_t));

static_assert(__is_constructible(__SVInt8_t));
static_assert(__is_constructible(__SVInt8_t, __SVInt8_t));
static_assert(!__is_constructible(__SVInt8_t, int));
static_assert(__is_nothrow_constructible(__SVInt8_t));
static_assert(__is_nothrow_constructible(__SVInt8_t, __SVInt8_t));
static_assert(!__is_nothrow_constructible(__SVInt8_t, int));
static_assert(__is_trivially_constructible(__SVInt8_t));
static_assert(__is_trivially_constructible(__SVInt8_t, __SVInt8_t));
static_assert(!__is_trivially_constructible(__SVInt8_t, int));

static_assert(__is_assignable(__SVInt8_t &, __SVInt8_t));
static_assert(__is_nothrow_assignable(__SVInt8_t &, __SVInt8_t));
static_assert(__is_trivially_assignable(__SVInt8_t &, __SVInt8_t));

static_assert(!__is_assignable(__SVInt8_t &, int));
static_assert(!__is_nothrow_assignable(__SVInt8_t &, int));
static_assert(!__is_trivially_assignable(__SVInt8_t &, int));

static_assert(__is_destructible(__SVInt8_t));
static_assert(__is_nothrow_destructible(__SVInt8_t));

#if defined(__clang__)
static_assert(!__is_array(__SVInt8_t));
static_assert(!__is_arithmetic(__SVInt8_t));
static_assert(__is_complete_type(__SVInt8_t));
static_assert(!__is_const(__SVInt8_t));
static_assert(!__is_floating_point(__SVInt8_t));
static_assert(!__is_fundamental(__SVInt8_t));
static_assert(!__is_integral(__SVInt8_t));
static_assert(!__is_lvalue_reference(__SVInt8_t));
static_assert(!__is_member_function_pointer(__SVInt8_t));
static_assert(!__is_member_object_pointer(__SVInt8_t));
static_assert(!__is_member_pointer(__SVInt8_t));
static_assert(__is_object(__SVInt8_t));
static_assert(!__is_pointer(__SVInt8_t));
static_assert(!__is_reference(__SVInt8_t));
static_assert(!__is_rvalue_reference(__SVInt8_t));
static_assert(!__is_scalar(__SVInt8_t));
static_assert(!__is_unsigned(__SVInt8_t));
static_assert(!__is_void(__SVInt8_t));
static_assert(!__is_volatile(__SVInt8_t));
static_assert(__is_same_as(__SVInt8_t, __SVInt8_t));
static_assert(!__is_same_as(__SVInt8_t, __SVInt16_t));
static_assert(!__is_bounded_array(__SVInt8_t));
static_assert(!__is_unbounded_array(__SVInt8_t));
static_assert(__is_referenceable(__SVInt8_t));
static_assert(!__is_literal_type(__SVInt8_t));
static_assert(__is_convertible(__SVInt8_t, __SVInt8_t));
static_assert(!__is_convertible(__SVInt8_t, int));
#endif

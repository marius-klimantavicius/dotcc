//type:fp
//options:--c++17 --clang_version 180100 --target linux_aarch64

struct B
{ };

void f(B b, const B cb, int i, const int ci, float f, const float cf)
{
  static_assert(__is_same(decltype(__builtin_arm_ldrex(&i)), int));
  static_assert(__is_same(decltype(__builtin_arm_ldrex(&f)), float));

  static_assert(__is_same(decltype(__builtin_arm_ldaex(&i)), int));
  static_assert(__is_same(decltype(__builtin_arm_ldaex(&f)), float));

  static_assert(__is_same(decltype(__builtin_arm_addg(&b, 0)), B *));
  static_assert(__is_same(decltype(__builtin_arm_addg(&cb, 0)), const B *));
  static_assert(__is_same(decltype(__builtin_arm_addg(&i, 0)), int *));
  static_assert(__is_same(decltype(__builtin_arm_addg(&ci, 0)), const int *));
  static_assert(__is_same(decltype(__builtin_arm_addg(&f, 0)), float *));
  static_assert(__is_same(decltype(__builtin_arm_addg(&cf, 0)), const float *));

  static_assert(__is_same(decltype(__builtin_arm_irg(&b, 0)), B *));
  static_assert(__is_same(decltype(__builtin_arm_irg(&cb, 0)), const B *));
  static_assert(__is_same(decltype(__builtin_arm_irg(&i, 0)), int *));
  static_assert(__is_same(decltype(__builtin_arm_irg(&ci, 0)), const int *));
  static_assert(__is_same(decltype(__builtin_arm_irg(&f, 0)), float *));
  static_assert(__is_same(decltype(__builtin_arm_irg(&cf, 0)), const float *));

  static_assert(__is_same(decltype(__builtin_arm_ldg(&b)), B *));
  static_assert(__is_same(decltype(__builtin_arm_ldg(&cb)), const B *));
  static_assert(__is_same(decltype(__builtin_arm_ldg(&i)), int *));
  static_assert(__is_same(decltype(__builtin_arm_ldg(&ci)), const int *));
  static_assert(__is_same(decltype(__builtin_arm_ldg(&f)), float *));
  static_assert(__is_same(decltype(__builtin_arm_ldg(&cf)), const float *));
}

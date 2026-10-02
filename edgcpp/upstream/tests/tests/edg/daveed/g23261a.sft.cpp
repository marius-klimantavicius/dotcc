//remark:C-mode conditional
//options:--c11 --gnu=90300;fp

#define __floating_type(type) \
  (__builtin_classify_type (__real__ ((type) 0)) == 8)

#define __tgmath_complex_type_sub(T, E1) \
  __typeof__ (*((__typeof__ (0 ? (T *) 0 : (void *) (!(E1)))) 0))

#define __tgmath_complex_type(expr) \
  __tgmath_complex_type_sub (_Complex double, \
                             __floating_type (_Complex double))

extern double acos (double);

_Complex double x, y;

void foo (void)
{
  x = __extension__ ((__tgmath_complex_type (y)) acos (y));
}

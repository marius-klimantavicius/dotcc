//type: fp
//options: 
# 0 "./warn/Wdouble-promotion.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/Wdouble-promotion.C"



# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 425 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
} max_align_t;






  typedef decltype(nullptr) nullptr_t;
# 5 "./warn/Wdouble-promotion.C" 2






# 10 "./warn/Wdouble-promotion.C"
float f;
double d;
int i;
long double ld;
_Complex float cf;
_Complex double cd;
_Complex long double cld;
size_t s;

extern void varargs_fn (int, ...);
extern void double_fn (double);
extern float float_fn (void);

void
usual_arithmetic_conversions(void)
{
  float local_f;
  _Complex float local_cf;




  local_f = f + 1.0;
  local_f = f - d;
  local_f = 1.0f * 1.0;
  local_f = 1.0f / d;

  local_cf = cf + 1.0;
  local_cf = cf - d;
  local_cf = cf + 1.0 * ((_Complex double)1.0iF);
  local_cf = cf - cd;

  local_f = i ? f : d;
  i = f == d;
  i = d != f;
}

void
default_argument_promotion (void)
{


  varargs_fn (1, f);
}




void
casts (void)
{
  float local_f;
  _Complex float local_cf;

  local_f = (double)f + 1.0;
  local_f = (double)f - d;
  local_f = (double)1.0f + 1.0;
  local_f = (double)1.0f - d;

  local_cf = (_Complex double)cf + 1.0;
  local_cf = (_Complex double)cf - d;
  local_cf = (_Complex double)cf + 1.0 * ((_Complex double)1.0iF);
  local_cf = (_Complex double)cf - cd;

  local_f = i ? (double)f : d;
  i = (double)f == d;
  i = d != (double)f;
}




void
assignments (void)
{
  d = f;
  double_fn (f);
  d = float_fn ();
}



void
non_evaluated (void)
{
  s = sizeof (f + 1.0);
  s = __alignof__ (f + 1.0);
  d = (__typeof__(f + 1.0))f;
  s = sizeof (i ? f : d);
}

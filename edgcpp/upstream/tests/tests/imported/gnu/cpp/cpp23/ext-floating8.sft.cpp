//type: rp
//options: --c++23
# 0 "./cpp23/ext-floating8.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp23/ext-floating8.C"
# 13 "./cpp23/ext-floating8.C"
# 1 "./cpp23/ext-floating7.C" 1
# 13 "./cpp23/ext-floating7.C"
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 14 "./cpp23/ext-floating7.C" 2
# 1 "./cpp23/ext-floating.h" 1



# 3 "./cpp23/ext-floating.h"
namespace std
{

  using float16_t = _Float16;


  using float32_t = _Float32;


  using float64_t = _Float64;


  using float128_t = _Float128;


  using bfloat16_t = decltype (0.0bf16);

  template<typename T, T v> struct integral_constant {
    static constexpr T value = v;
  };
  typedef integral_constant<bool, false> false_type;
  typedef integral_constant<bool, true> true_type;
  template<class T, class U>
  struct is_same : std::false_type {};
  template <class T>
  struct is_same<T, T> : std::true_type {};
}
# 15 "./cpp23/ext-floating7.C" 2
# 23 "./cpp23/ext-floating7.C"
extern "C" void abort ();

volatile _Float32 a = 1.0f32, b = 2.5F32, c = -2.5f32;
volatile _Float32 a2 = 1.0f32, z = 0.0f32, nz = -0.0f32;



_Float32
vafn (_Float32 arg1, ...)
{
  va_list ap;
  _Float32 ret;
  
# 35 "./cpp23/ext-floating7.C" 3 4
 __builtin_va_start(
# 35 "./cpp23/ext-floating7.C"
 ap
# 35 "./cpp23/ext-floating7.C" 3 4
 ,
# 35 "./cpp23/ext-floating7.C"
 arg1
# 35 "./cpp23/ext-floating7.C" 3 4
 )
# 35 "./cpp23/ext-floating7.C"
                    ;
  ret = arg1 + 
# 36 "./cpp23/ext-floating7.C" 3 4
              __builtin_va_arg(
# 36 "./cpp23/ext-floating7.C"
              ap
# 36 "./cpp23/ext-floating7.C" 3 4
              ,
# 36 "./cpp23/ext-floating7.C"
              _Float32
# 36 "./cpp23/ext-floating7.C" 3 4
              )
# 36 "./cpp23/ext-floating7.C"
                               ;
  
# 37 "./cpp23/ext-floating7.C" 3 4
 __builtin_va_end(
# 37 "./cpp23/ext-floating7.C"
 ap
# 37 "./cpp23/ext-floating7.C" 3 4
 )
# 37 "./cpp23/ext-floating7.C"
            ;
  return ret;
}

_Float32
fn (_Float32 arg)
{
  return arg / 4;
}

int
main (void)
{
  volatile _Float32 r;
  r = -b;
  if (r != c)
    abort ();
  r = a + b;
  if (r != 3.5f32)
    abort ();
  r = a - b;
  if (r != -1.5f32)
    abort ();
  r = 2 * c;
  if (r != -5)
    abort ();
  r = b * c;
  if (r != -6.25f32)
    abort ();
  r = b / (a + a);
  if (r != 1.25f32)
    abort ();
  r = c * 3;
  if (r != -7.5f32)
    abort ();
  volatile int i = r;
  if (i != -7)
    abort ();
  r = vafn (a, c);
  if (r != -1.5f32)
    abort ();
  r = fn (a);
  if (r != 0.25f32)
    abort ();
  if ((a < b) != 1)
    abort ();
  if ((b < a) != 0)
    abort ();
  if ((a < a2) != 0)
    abort ();
  if ((nz < z) != 0)
    abort ();
  if ((a <= b) != 1)
    abort ();
  if ((b <= a) != 0)
    abort ();
  if ((a <= a2) != 1)
    abort ();
  if ((nz <= z) != 1)
    abort ();
  if ((a > b) != 0)
    abort ();
  if ((b > a) != 1)
    abort ();
  if ((a > a2) != 0)
    abort ();
  if ((nz > z) != 0)
    abort ();
  if ((a >= b) != 0)
    abort ();
  if ((b >= a) != 1)
    abort ();
  if ((a >= a2) != 1)
    abort ();
  if ((nz >= z) != 1)
    abort ();
  i = (nz == z);
  if (i != 1)
    abort ();
  i = (a == b);
  if (i != 0)
    abort ();
}
# 14 "./cpp23/ext-floating8.C" 2

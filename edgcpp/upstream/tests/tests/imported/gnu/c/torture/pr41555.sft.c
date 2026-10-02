//type: rp
//options: --c99
# 0 "./torture/pr41555.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/pr41555.c"



# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 1 3 4
# 9 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 3 4
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/usr/include/stdint.h" 1 3 4
# 25 "/usr/include/stdint.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
# 375 "/usr/include/features.h" 3 4
# 1 "/usr/include/sys/cdefs.h" 1 3 4
# 392 "/usr/include/sys/cdefs.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 393 "/usr/include/sys/cdefs.h" 2 3 4
# 376 "/usr/include/features.h" 2 3 4
# 399 "/usr/include/features.h" 3 4
# 1 "/usr/include/gnu/stubs.h" 1 3 4
# 10 "/usr/include/gnu/stubs.h" 3 4
# 1 "/usr/include/gnu/stubs-64.h" 1 3 4
# 11 "/usr/include/gnu/stubs.h" 2 3 4
# 400 "/usr/include/features.h" 2 3 4
# 26 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wchar.h" 1 3 4
# 22 "/usr/include/bits/wchar.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 23 "/usr/include/bits/wchar.h" 2 3 4
# 27 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 28 "/usr/include/stdint.h" 2 3 4
# 36 "/usr/include/stdint.h" 3 4
typedef signed char int8_t;
typedef short int int16_t;
typedef int int32_t;

typedef long int int64_t;







typedef unsigned char uint8_t;
typedef unsigned short int uint16_t;

typedef unsigned int uint32_t;



typedef unsigned long int uint64_t;
# 65 "/usr/include/stdint.h" 3 4
typedef signed char int_least8_t;
typedef short int int_least16_t;
typedef int int_least32_t;

typedef long int int_least64_t;






typedef unsigned char uint_least8_t;
typedef unsigned short int uint_least16_t;
typedef unsigned int uint_least32_t;

typedef unsigned long int uint_least64_t;
# 90 "/usr/include/stdint.h" 3 4
typedef signed char int_fast8_t;

typedef long int int_fast16_t;
typedef long int int_fast32_t;
typedef long int int_fast64_t;
# 103 "/usr/include/stdint.h" 3 4
typedef unsigned char uint_fast8_t;

typedef unsigned long int uint_fast16_t;
typedef unsigned long int uint_fast32_t;
typedef unsigned long int uint_fast64_t;
# 119 "/usr/include/stdint.h" 3 4
typedef long int intptr_t;


typedef unsigned long int uintptr_t;
# 134 "/usr/include/stdint.h" 3 4
typedef long int intmax_t;
typedef unsigned long int uintmax_t;
# 12 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 2 3 4
#pragma GCC diagnostic pop
# 5 "./torture/pr41555.c" 2
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 1 3 4
# 34 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/syslimits.h" 1 3 4






#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 1 3 4
# 210 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 3 4
# 1 "/usr/include/limits.h" 1 3 4
# 211 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 2 3 4
# 10 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/syslimits.h" 2 3 4
#pragma GCC diagnostic pop
# 35 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 2 3 4
# 6 "./torture/pr41555.c" 2


# 7 "./torture/pr41555.c"
extern void abort (void);

static uint64_t safe_div_func_uint64_t_u_u (uint64_t _ui1, uint64_t _ui2)
{
  if (_ui2==0) return _ui1;
  return _ui1 / _ui2;
}

static int64_t safe_div_func_int64_t_s_s (int64_t _si1, int64_t _si2)
{
  if (_si2==0 || (_si1==
# 17 "./torture/pr41555.c" 3 4
                       (-9223372036854775807L -1) 
# 17 "./torture/pr41555.c"
                                 && _si2==-1)) return _si1;
  return _si1 / _si2;
}
# 28 "./torture/pr41555.c"
static int8_t
safe_add_func_int8_t_s_s(int8_t _si1, int8_t _si2)
{
  return ((((((int8_t)(_si1))>((int8_t)0)) && (((int8_t)(_si2))>((int8_t)0)) && (((int8_t)(_si1)) > ((
# 31 "./torture/pr41555.c" 3 4
        (127)
# 31 "./torture/pr41555.c"
        )-((int8_t)(_si2))))) || ((((int8_t)(_si1))<((int8_t)0)) && (((int8_t)(_si2))<((int8_t)0)) && (((int8_t)(_si1)) < ((
# 31 "./torture/pr41555.c" 3 4
        (-128)
# 31 "./torture/pr41555.c"
        )-((int8_t)(_si2)))))) ? ((int8_t)(_si1)) : (((int8_t)(_si1)) + ((int8_t)(_si2))) );
}







static uint64_t
safe_rshift_func_uint64_t_u_s(uint64_t _left, int _right)
{
  return (((((int)(_right)) < ((uint64_t)0)) || (((int)(_right)) >= sizeof(uint64_t)*8
# 43 "./torture/pr41555.c"
        )) ? ((uint64_t)(_left)) : (((uint64_t)(_left)) >> ((int)(_right))));
}
# 54 "./torture/pr41555.c"
static int32_t
safe_mul_func_int32_t_s_s (int32_t _si1, int32_t _si2)
{
  return ((((((int32_t)(_si1)) > ((int32_t)0)) && (((int32_t)(_si2)) > ((int32_t)0)) && (((int32_t)(_si1)) > ((
# 57 "./torture/pr41555.c" 3 4
        (2147483647)
# 57 "./torture/pr41555.c"
        ) / ((int32_t)(_si2))))) || ((((int32_t)(_si1)) > ((int32_t)0)) && (((int32_t)(_si2)) <= ((int32_t)0)) && (((int32_t)(_si2)) < ((
# 57 "./torture/pr41555.c" 3 4
        (-2147483647-1)
# 57 "./torture/pr41555.c"
        ) / ((int32_t)(_si1))))) || ((((int32_t)(_si1)) <= ((int32_t)0)) && (((int32_t)(_si2)) > ((int32_t)0)) && (((int32_t)(_si1)) < ((
# 57 "./torture/pr41555.c" 3 4
        (-2147483647-1)
# 57 "./torture/pr41555.c"
        ) / ((int32_t)(_si2))))) || ((((int32_t)(_si1)) <= ((int32_t)0)) && (((int32_t)(_si2)) <= ((int32_t)0)) && (((int32_t)(_si1)) != ((int32_t)0)) && (((int32_t)(_si2)) < ((
# 57 "./torture/pr41555.c" 3 4
        (2147483647)
# 57 "./torture/pr41555.c"
        ) / ((int32_t)(_si1)))))) ? ((int32_t)(_si1)) : ((int32_t)(_si1)) * ((int32_t)(_si2)));
}

static int8_t g_39;
static volatile uint8_t g_46;
static uint8_t g_47;
static uint8_t *g_62;
static uint8_t g_79;
static int8_t g_101 = -1L;
static uint8_t *g_114;
static uint8_t *g_126;
static uint8_t g_133;

static uint16_t func_35 (int32_t * p_36, uint64_t p_37, uint32_t p_38);
static uint16_t func_35 (int32_t * p_36, uint64_t p_37, uint32_t p_38)
{
  if (g_62 != 0)
    abort ();
  for (g_39 = 1; g_39 < 0; g_39 = 1)
    {
    }
  return 1;
}

static int32_t func_19 (int32_t p_20);
static int32_t func_19 (int32_t p_20)
{
  if (1 !=
      safe_div_func_uint64_t_u_u ((safe_div_func_int64_t_s_s (p_20, 1)),
                                  g_101))
    {
      func_35 (0, 1 <= (safe_add_func_int8_t_s_s (g_47, g_46)) > p_20 < 1, 1);
      g_133 = 1;
      if (g_114 != 0)
 abort ();
      if (g_126 != 0)
 abort ();
    }
  return 1;
}

static uint8_t func_2 (int32_t p_6);
static uint8_t func_2 (int32_t p_6)
{
  for (1; p_6 > 1; 1)
    return 0;
  func_19 (g_79);
  if (safe_mul_func_int32_t_s_s
      ((0, 1 < (safe_rshift_func_uint64_t_u_s (1 ^ p_6, 1))),
       (func_35 (&p_6, 1, 1) < 1)))
    {
    }
  return 1;
}

int main (void)
{
  func_2 (1);
  if (g_133 != 1)
    abort ();
  return 0;
}

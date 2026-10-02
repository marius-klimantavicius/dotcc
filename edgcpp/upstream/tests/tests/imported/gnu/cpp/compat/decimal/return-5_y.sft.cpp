//type: fp
//options: 
# 0 "./compat/decimal/return-5_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/decimal/return-5_y.C"
# 1 "./compat/decimal/decimal-dummy.h" 1
namespace std {
namespace decimal {

  class decimal32
  {
  public:
    typedef float __dec32 __attribute__((mode(SD)));
    decimal32 () : __val(0.e-101DF) {}
    decimal32 (__dec32 x) : __val(x) {}
    __dec32 __val;
  };

  class decimal64
  {
  public:
    typedef float __dec64 __attribute__((mode(DD)));
    decimal64 () : __val(0.e-398dd) {}
    decimal64 (__dec64 x) : __val(x) {}
    __dec64 __val;
  };

  class decimal128
  {
  public:
    typedef float __dec128 __attribute__((mode(TD)));
    decimal128 () : __val(0.e-6176DL) {}
    decimal128 (__dec128 x) : __val(x) {}
    __dec128 __val;
  };

  inline decimal32 operator+ (decimal32 lhs, decimal32 rhs)
  {
    decimal32 tmp;
    tmp.__val = lhs.__val + rhs.__val;
    return tmp;
  }

  inline decimal64 operator+ (decimal64 lhs, decimal64 rhs)
  {
    decimal64 tmp;
    tmp.__val = lhs.__val + rhs.__val;
    return tmp;
  }

  inline decimal128 operator+ (decimal128 lhs, decimal128 rhs)
  {
    decimal128 tmp;
    tmp.__val = lhs.__val + rhs.__val;
    return tmp;
  }

  inline bool operator!= (decimal32 lhs, decimal32 rhs)
  {
    return lhs.__val != rhs.__val;
  }

  inline bool operator!= (decimal64 lhs, decimal64 rhs)
  {
    return lhs.__val != rhs.__val;
  }

  inline bool operator!= (decimal128 lhs, decimal128 rhs)
  {
    return lhs.__val != rhs.__val;
  }
}
}
# 2 "./compat/decimal/return-5_y.C" 2

typedef std::decimal::decimal32 dec32;
typedef std::decimal::decimal64 dec64;
typedef std::decimal::decimal128 dec128;

# 1 "./compat/decimal/return_y.h" 1
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 2 "./compat/decimal/return_y.h" 2

# 1 "./compat/decimal/compat-common.h" 1
# 51 "./compat/decimal/compat-common.h"

# 51 "./compat/decimal/compat-common.h"
extern "C" void abort (void);



extern int fails;
# 4 "./compat/decimal/return_y.h" 2
# 65 "./compat/decimal/return_y.h"
extern dec32 g01d32, g02d32, g03d32, g04d32; extern dec32 g05d32, g06d32, g07d32, g08d32; extern dec32 g09d32, g10d32, g11d32, g12d32; extern dec32 g13d32, g14d32, g15d32, g16d32; extern void checkd32 (dec32 x, dec32 v); extern void initd32 (dec32 *p, dec32 v) { *p = v + (dec32)1.5; } extern void checkgd32 (void) { checkd32 (g01d32, 1+(dec32)1.5); checkd32 (g02d32, 2+(dec32)1.5); checkd32 (g03d32, 3+(dec32)1.5); checkd32 (g04d32, 4+(dec32)1.5); checkd32 (g05d32, 5+(dec32)1.5); checkd32 (g06d32, 6+(dec32)1.5); checkd32 (g07d32, 7+(dec32)1.5); checkd32 (g08d32, 8+(dec32)1.5); checkd32 (g09d32, 9+(dec32)1.5); checkd32 (g10d32, 10+(dec32)1.5); checkd32 (g11d32, 11+(dec32)1.5); checkd32 (g12d32, 12+(dec32)1.5); checkd32 (g13d32, 13+(dec32)1.5); checkd32 (g14d32, 14+(dec32)1.5); checkd32 (g15d32, 15+(dec32)1.5); checkd32 (g16d32, 16+(dec32)1.5); } extern dec32 test0d32 (void) { return g01d32; } extern dec32 test1d32 (dec32 x01) { return x01; } extern dec32 testvad32 (int n, ...) { int i; dec32 rslt; va_list ap; 
# 65 "./compat/decimal/return_y.h" 3 4
__builtin_va_start(
# 65 "./compat/decimal/return_y.h"
ap
# 65 "./compat/decimal/return_y.h" 3 4
,
# 65 "./compat/decimal/return_y.h"
n
# 65 "./compat/decimal/return_y.h" 3 4
)
# 65 "./compat/decimal/return_y.h"
; for (i = 0; i < n; i++) rslt = 
# 65 "./compat/decimal/return_y.h" 3 4
__builtin_va_arg(
# 65 "./compat/decimal/return_y.h"
ap
# 65 "./compat/decimal/return_y.h" 3 4
,
# 65 "./compat/decimal/return_y.h"
dec32
# 65 "./compat/decimal/return_y.h" 3 4
)
# 65 "./compat/decimal/return_y.h"
; 
# 65 "./compat/decimal/return_y.h" 3 4
__builtin_va_end(
# 65 "./compat/decimal/return_y.h"
ap
# 65 "./compat/decimal/return_y.h" 3 4
)
# 65 "./compat/decimal/return_y.h"
; return rslt; }
extern dec64 g01d64, g02d64, g03d64, g04d64; extern dec64 g05d64, g06d64, g07d64, g08d64; extern dec64 g09d64, g10d64, g11d64, g12d64; extern dec64 g13d64, g14d64, g15d64, g16d64; extern void checkd64 (dec64 x, dec64 v); extern void initd64 (dec64 *p, dec64 v) { *p = v + (dec64)2.5; } extern void checkgd64 (void) { checkd64 (g01d64, 1+(dec64)2.5); checkd64 (g02d64, 2+(dec64)2.5); checkd64 (g03d64, 3+(dec64)2.5); checkd64 (g04d64, 4+(dec64)2.5); checkd64 (g05d64, 5+(dec64)2.5); checkd64 (g06d64, 6+(dec64)2.5); checkd64 (g07d64, 7+(dec64)2.5); checkd64 (g08d64, 8+(dec64)2.5); checkd64 (g09d64, 9+(dec64)2.5); checkd64 (g10d64, 10+(dec64)2.5); checkd64 (g11d64, 11+(dec64)2.5); checkd64 (g12d64, 12+(dec64)2.5); checkd64 (g13d64, 13+(dec64)2.5); checkd64 (g14d64, 14+(dec64)2.5); checkd64 (g15d64, 15+(dec64)2.5); checkd64 (g16d64, 16+(dec64)2.5); } extern dec64 test0d64 (void) { return g01d64; } extern dec64 test1d64 (dec64 x01) { return x01; } extern dec64 testvad64 (int n, ...) { int i; dec64 rslt; va_list ap; 
# 66 "./compat/decimal/return_y.h" 3 4
__builtin_va_start(
# 66 "./compat/decimal/return_y.h"
ap
# 66 "./compat/decimal/return_y.h" 3 4
,
# 66 "./compat/decimal/return_y.h"
n
# 66 "./compat/decimal/return_y.h" 3 4
)
# 66 "./compat/decimal/return_y.h"
; for (i = 0; i < n; i++) rslt = 
# 66 "./compat/decimal/return_y.h" 3 4
__builtin_va_arg(
# 66 "./compat/decimal/return_y.h"
ap
# 66 "./compat/decimal/return_y.h" 3 4
,
# 66 "./compat/decimal/return_y.h"
dec64
# 66 "./compat/decimal/return_y.h" 3 4
)
# 66 "./compat/decimal/return_y.h"
; 
# 66 "./compat/decimal/return_y.h" 3 4
__builtin_va_end(
# 66 "./compat/decimal/return_y.h"
ap
# 66 "./compat/decimal/return_y.h" 3 4
)
# 66 "./compat/decimal/return_y.h"
; return rslt; }
extern dec128 g01d128, g02d128, g03d128, g04d128; extern dec128 g05d128, g06d128, g07d128, g08d128; extern dec128 g09d128, g10d128, g11d128, g12d128; extern dec128 g13d128, g14d128, g15d128, g16d128; extern void checkd128 (dec128 x, dec128 v); extern void initd128 (dec128 *p, dec128 v) { *p = v + (dec128)3.5; } extern void checkgd128 (void) { checkd128 (g01d128, 1+(dec128)3.5); checkd128 (g02d128, 2+(dec128)3.5); checkd128 (g03d128, 3+(dec128)3.5); checkd128 (g04d128, 4+(dec128)3.5); checkd128 (g05d128, 5+(dec128)3.5); checkd128 (g06d128, 6+(dec128)3.5); checkd128 (g07d128, 7+(dec128)3.5); checkd128 (g08d128, 8+(dec128)3.5); checkd128 (g09d128, 9+(dec128)3.5); checkd128 (g10d128, 10+(dec128)3.5); checkd128 (g11d128, 11+(dec128)3.5); checkd128 (g12d128, 12+(dec128)3.5); checkd128 (g13d128, 13+(dec128)3.5); checkd128 (g14d128, 14+(dec128)3.5); checkd128 (g15d128, 15+(dec128)3.5); checkd128 (g16d128, 16+(dec128)3.5); } extern dec128 test0d128 (void) { return g01d128; } extern dec128 test1d128 (dec128 x01) { return x01; } extern dec128 testvad128 (int n, ...) { int i; dec128 rslt; va_list ap; 
# 67 "./compat/decimal/return_y.h" 3 4
__builtin_va_start(
# 67 "./compat/decimal/return_y.h"
ap
# 67 "./compat/decimal/return_y.h" 3 4
,
# 67 "./compat/decimal/return_y.h"
n
# 67 "./compat/decimal/return_y.h" 3 4
)
# 67 "./compat/decimal/return_y.h"
; for (i = 0; i < n; i++) rslt = 
# 67 "./compat/decimal/return_y.h" 3 4
__builtin_va_arg(
# 67 "./compat/decimal/return_y.h"
ap
# 67 "./compat/decimal/return_y.h" 3 4
,
# 67 "./compat/decimal/return_y.h"
dec128
# 67 "./compat/decimal/return_y.h" 3 4
)
# 67 "./compat/decimal/return_y.h"
; 
# 67 "./compat/decimal/return_y.h" 3 4
__builtin_va_end(
# 67 "./compat/decimal/return_y.h"
ap
# 67 "./compat/decimal/return_y.h" 3 4
)
# 67 "./compat/decimal/return_y.h"
; return rslt; }
# 8 "./compat/decimal/return-5_y.C" 2

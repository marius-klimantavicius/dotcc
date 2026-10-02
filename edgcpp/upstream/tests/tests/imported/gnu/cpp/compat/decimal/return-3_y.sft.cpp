//type: fp
//options: 
# 0 "./compat/decimal/return-3_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/decimal/return-3_y.C"
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
# 2 "./compat/decimal/return-3_y.C" 2





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
extern std::decimal::decimal32 g01d32, g02d32, g03d32, g04d32; extern std::decimal::decimal32 g05d32, g06d32, g07d32, g08d32; extern std::decimal::decimal32 g09d32, g10d32, g11d32, g12d32; extern std::decimal::decimal32 g13d32, g14d32, g15d32, g16d32; extern void checkd32 (std::decimal::decimal32 x, std::decimal::decimal32 v); extern void initd32 (std::decimal::decimal32 *p, std::decimal::decimal32 v) { *p = v + (std::decimal::decimal32)1.5; } extern void checkgd32 (void) { checkd32 (g01d32, 1+(std::decimal::decimal32)1.5); checkd32 (g02d32, 2+(std::decimal::decimal32)1.5); checkd32 (g03d32, 3+(std::decimal::decimal32)1.5); checkd32 (g04d32, 4+(std::decimal::decimal32)1.5); checkd32 (g05d32, 5+(std::decimal::decimal32)1.5); checkd32 (g06d32, 6+(std::decimal::decimal32)1.5); checkd32 (g07d32, 7+(std::decimal::decimal32)1.5); checkd32 (g08d32, 8+(std::decimal::decimal32)1.5); checkd32 (g09d32, 9+(std::decimal::decimal32)1.5); checkd32 (g10d32, 10+(std::decimal::decimal32)1.5); checkd32 (g11d32, 11+(std::decimal::decimal32)1.5); checkd32 (g12d32, 12+(std::decimal::decimal32)1.5); checkd32 (g13d32, 13+(std::decimal::decimal32)1.5); checkd32 (g14d32, 14+(std::decimal::decimal32)1.5); checkd32 (g15d32, 15+(std::decimal::decimal32)1.5); checkd32 (g16d32, 16+(std::decimal::decimal32)1.5); } extern std::decimal::decimal32 test0d32 (void) { return g01d32; } extern std::decimal::decimal32 test1d32 (std::decimal::decimal32 x01) { return x01; } extern std::decimal::decimal32 testvad32 (int n, ...) { int i; std::decimal::decimal32 rslt; va_list ap; 
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
std::decimal::decimal32
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
extern std::decimal::decimal64 g01d64, g02d64, g03d64, g04d64; extern std::decimal::decimal64 g05d64, g06d64, g07d64, g08d64; extern std::decimal::decimal64 g09d64, g10d64, g11d64, g12d64; extern std::decimal::decimal64 g13d64, g14d64, g15d64, g16d64; extern void checkd64 (std::decimal::decimal64 x, std::decimal::decimal64 v); extern void initd64 (std::decimal::decimal64 *p, std::decimal::decimal64 v) { *p = v + (std::decimal::decimal64)2.5; } extern void checkgd64 (void) { checkd64 (g01d64, 1+(std::decimal::decimal64)2.5); checkd64 (g02d64, 2+(std::decimal::decimal64)2.5); checkd64 (g03d64, 3+(std::decimal::decimal64)2.5); checkd64 (g04d64, 4+(std::decimal::decimal64)2.5); checkd64 (g05d64, 5+(std::decimal::decimal64)2.5); checkd64 (g06d64, 6+(std::decimal::decimal64)2.5); checkd64 (g07d64, 7+(std::decimal::decimal64)2.5); checkd64 (g08d64, 8+(std::decimal::decimal64)2.5); checkd64 (g09d64, 9+(std::decimal::decimal64)2.5); checkd64 (g10d64, 10+(std::decimal::decimal64)2.5); checkd64 (g11d64, 11+(std::decimal::decimal64)2.5); checkd64 (g12d64, 12+(std::decimal::decimal64)2.5); checkd64 (g13d64, 13+(std::decimal::decimal64)2.5); checkd64 (g14d64, 14+(std::decimal::decimal64)2.5); checkd64 (g15d64, 15+(std::decimal::decimal64)2.5); checkd64 (g16d64, 16+(std::decimal::decimal64)2.5); } extern std::decimal::decimal64 test0d64 (void) { return g01d64; } extern std::decimal::decimal64 test1d64 (std::decimal::decimal64 x01) { return x01; } extern std::decimal::decimal64 testvad64 (int n, ...) { int i; std::decimal::decimal64 rslt; va_list ap; 
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
std::decimal::decimal64
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
extern std::decimal::decimal128 g01d128, g02d128, g03d128, g04d128; extern std::decimal::decimal128 g05d128, g06d128, g07d128, g08d128; extern std::decimal::decimal128 g09d128, g10d128, g11d128, g12d128; extern std::decimal::decimal128 g13d128, g14d128, g15d128, g16d128; extern void checkd128 (std::decimal::decimal128 x, std::decimal::decimal128 v); extern void initd128 (std::decimal::decimal128 *p, std::decimal::decimal128 v) { *p = v + (std::decimal::decimal128)3.5; } extern void checkgd128 (void) { checkd128 (g01d128, 1+(std::decimal::decimal128)3.5); checkd128 (g02d128, 2+(std::decimal::decimal128)3.5); checkd128 (g03d128, 3+(std::decimal::decimal128)3.5); checkd128 (g04d128, 4+(std::decimal::decimal128)3.5); checkd128 (g05d128, 5+(std::decimal::decimal128)3.5); checkd128 (g06d128, 6+(std::decimal::decimal128)3.5); checkd128 (g07d128, 7+(std::decimal::decimal128)3.5); checkd128 (g08d128, 8+(std::decimal::decimal128)3.5); checkd128 (g09d128, 9+(std::decimal::decimal128)3.5); checkd128 (g10d128, 10+(std::decimal::decimal128)3.5); checkd128 (g11d128, 11+(std::decimal::decimal128)3.5); checkd128 (g12d128, 12+(std::decimal::decimal128)3.5); checkd128 (g13d128, 13+(std::decimal::decimal128)3.5); checkd128 (g14d128, 14+(std::decimal::decimal128)3.5); checkd128 (g15d128, 15+(std::decimal::decimal128)3.5); checkd128 (g16d128, 16+(std::decimal::decimal128)3.5); } extern std::decimal::decimal128 test0d128 (void) { return g01d128; } extern std::decimal::decimal128 test1d128 (std::decimal::decimal128 x01) { return x01; } extern std::decimal::decimal128 testvad128 (int n, ...) { int i; std::decimal::decimal128 rslt; va_list ap; 
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
std::decimal::decimal128
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
# 8 "./compat/decimal/return-3_y.C" 2

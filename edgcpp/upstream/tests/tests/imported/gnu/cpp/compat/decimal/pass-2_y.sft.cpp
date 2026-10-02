//type: fp
//options: 
# 0 "./compat/decimal/pass-2_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/decimal/pass-2_y.C"
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
# 2 "./compat/decimal/pass-2_y.C" 2





# 1 "./compat/decimal/pass_y.h" 1
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 2 "./compat/decimal/pass_y.h" 2

# 1 "./compat/decimal/compat-common.h" 1
# 51 "./compat/decimal/compat-common.h"

# 51 "./compat/decimal/compat-common.h"
extern "C" void abort (void);



extern int fails;
# 4 "./compat/decimal/pass_y.h" 2




const int test_va = 1;
# 87 "./compat/decimal/pass_y.h"
extern std::decimal::decimal32 g01d32, g02d32, g03d32, g04d32; extern std::decimal::decimal32 g05d32, g06d32, g07d32, g08d32; extern std::decimal::decimal32 g09d32, g10d32, g11d32, g12d32; extern std::decimal::decimal32 g13d32, g14d32, g15d32, g16d32; extern void checkd32 (std::decimal::decimal32 x, std::decimal::decimal32 v); extern void initd32 (std::decimal::decimal32 *p, std::decimal::decimal32 v) { *p = v + (std::decimal::decimal32)1.5DF; } extern void checkgd32 (void) { checkd32 (g01d32, 1); checkd32 (g02d32, 2); checkd32 (g03d32, 3); checkd32 (g04d32, 4); checkd32 (g05d32, 5); checkd32 (g06d32, 6); checkd32 (g07d32, 7); checkd32 (g08d32, 8); checkd32 (g09d32, 9); checkd32 (g10d32, 10); checkd32 (g11d32, 11); checkd32 (g12d32, 12); checkd32 (g13d32, 13); checkd32 (g14d32, 14); checkd32 (g15d32, 15); checkd32 (g16d32, 16); } extern void testd32 (std::decimal::decimal32 x01, std::decimal::decimal32 x02, std::decimal::decimal32 x03, std::decimal::decimal32 x04, std::decimal::decimal32 x05, std::decimal::decimal32 x06, std::decimal::decimal32 x07, std::decimal::decimal32 x08, std::decimal::decimal32 x09, std::decimal::decimal32 x10, std::decimal::decimal32 x11, std::decimal::decimal32 x12, std::decimal::decimal32 x13, std::decimal::decimal32 x14, std::decimal::decimal32 x15, std::decimal::decimal32 x16) { checkd32 (x01, 1); checkd32 (x02, 2); checkd32 (x03, 3); checkd32 (x04, 4); checkd32 (x05, 5); checkd32 (x06, 6); checkd32 (x07, 7); checkd32 (x08, 8); checkd32 (x09, 9); checkd32 (x10, 10); checkd32 (x11, 11); checkd32 (x12, 12); checkd32 (x13, 13); checkd32 (x14, 14); checkd32 (x15, 15); checkd32 (x16, 16); } extern void testvad32 (int n, ...) { int i; va_list ap; if (test_va) { 
# 87 "./compat/decimal/pass_y.h" 3 4
__builtin_va_start(
# 87 "./compat/decimal/pass_y.h"
ap
# 87 "./compat/decimal/pass_y.h" 3 4
,
# 87 "./compat/decimal/pass_y.h"
n
# 87 "./compat/decimal/pass_y.h" 3 4
)
# 87 "./compat/decimal/pass_y.h"
; for (i = 0; i < n; i++) { std::decimal::decimal32 t = 
# 87 "./compat/decimal/pass_y.h" 3 4
__builtin_va_arg(
# 87 "./compat/decimal/pass_y.h"
ap
# 87 "./compat/decimal/pass_y.h" 3 4
,
# 87 "./compat/decimal/pass_y.h"
std::decimal::decimal32
# 87 "./compat/decimal/pass_y.h" 3 4
)
# 87 "./compat/decimal/pass_y.h"
; checkd32 (t, i+1); } 
# 87 "./compat/decimal/pass_y.h" 3 4
__builtin_va_end(
# 87 "./compat/decimal/pass_y.h"
ap
# 87 "./compat/decimal/pass_y.h" 3 4
)
# 87 "./compat/decimal/pass_y.h"
; } }
extern std::decimal::decimal64 g01d64, g02d64, g03d64, g04d64; extern std::decimal::decimal64 g05d64, g06d64, g07d64, g08d64; extern std::decimal::decimal64 g09d64, g10d64, g11d64, g12d64; extern std::decimal::decimal64 g13d64, g14d64, g15d64, g16d64; extern void checkd64 (std::decimal::decimal64 x, std::decimal::decimal64 v); extern void initd64 (std::decimal::decimal64 *p, std::decimal::decimal64 v) { *p = v + (std::decimal::decimal64)2.5DD; } extern void checkgd64 (void) { checkd64 (g01d64, 1); checkd64 (g02d64, 2); checkd64 (g03d64, 3); checkd64 (g04d64, 4); checkd64 (g05d64, 5); checkd64 (g06d64, 6); checkd64 (g07d64, 7); checkd64 (g08d64, 8); checkd64 (g09d64, 9); checkd64 (g10d64, 10); checkd64 (g11d64, 11); checkd64 (g12d64, 12); checkd64 (g13d64, 13); checkd64 (g14d64, 14); checkd64 (g15d64, 15); checkd64 (g16d64, 16); } extern void testd64 (std::decimal::decimal64 x01, std::decimal::decimal64 x02, std::decimal::decimal64 x03, std::decimal::decimal64 x04, std::decimal::decimal64 x05, std::decimal::decimal64 x06, std::decimal::decimal64 x07, std::decimal::decimal64 x08, std::decimal::decimal64 x09, std::decimal::decimal64 x10, std::decimal::decimal64 x11, std::decimal::decimal64 x12, std::decimal::decimal64 x13, std::decimal::decimal64 x14, std::decimal::decimal64 x15, std::decimal::decimal64 x16) { checkd64 (x01, 1); checkd64 (x02, 2); checkd64 (x03, 3); checkd64 (x04, 4); checkd64 (x05, 5); checkd64 (x06, 6); checkd64 (x07, 7); checkd64 (x08, 8); checkd64 (x09, 9); checkd64 (x10, 10); checkd64 (x11, 11); checkd64 (x12, 12); checkd64 (x13, 13); checkd64 (x14, 14); checkd64 (x15, 15); checkd64 (x16, 16); } extern void testvad64 (int n, ...) { int i; va_list ap; if (test_va) { 
# 88 "./compat/decimal/pass_y.h" 3 4
__builtin_va_start(
# 88 "./compat/decimal/pass_y.h"
ap
# 88 "./compat/decimal/pass_y.h" 3 4
,
# 88 "./compat/decimal/pass_y.h"
n
# 88 "./compat/decimal/pass_y.h" 3 4
)
# 88 "./compat/decimal/pass_y.h"
; for (i = 0; i < n; i++) { std::decimal::decimal64 t = 
# 88 "./compat/decimal/pass_y.h" 3 4
__builtin_va_arg(
# 88 "./compat/decimal/pass_y.h"
ap
# 88 "./compat/decimal/pass_y.h" 3 4
,
# 88 "./compat/decimal/pass_y.h"
std::decimal::decimal64
# 88 "./compat/decimal/pass_y.h" 3 4
)
# 88 "./compat/decimal/pass_y.h"
; checkd64 (t, i+1); } 
# 88 "./compat/decimal/pass_y.h" 3 4
__builtin_va_end(
# 88 "./compat/decimal/pass_y.h"
ap
# 88 "./compat/decimal/pass_y.h" 3 4
)
# 88 "./compat/decimal/pass_y.h"
; } }
extern std::decimal::decimal128 g01d128, g02d128, g03d128, g04d128; extern std::decimal::decimal128 g05d128, g06d128, g07d128, g08d128; extern std::decimal::decimal128 g09d128, g10d128, g11d128, g12d128; extern std::decimal::decimal128 g13d128, g14d128, g15d128, g16d128; extern void checkd128 (std::decimal::decimal128 x, std::decimal::decimal128 v); extern void initd128 (std::decimal::decimal128 *p, std::decimal::decimal128 v) { *p = v + (std::decimal::decimal128)3.5DL; } extern void checkgd128 (void) { checkd128 (g01d128, 1); checkd128 (g02d128, 2); checkd128 (g03d128, 3); checkd128 (g04d128, 4); checkd128 (g05d128, 5); checkd128 (g06d128, 6); checkd128 (g07d128, 7); checkd128 (g08d128, 8); checkd128 (g09d128, 9); checkd128 (g10d128, 10); checkd128 (g11d128, 11); checkd128 (g12d128, 12); checkd128 (g13d128, 13); checkd128 (g14d128, 14); checkd128 (g15d128, 15); checkd128 (g16d128, 16); } extern void testd128 (std::decimal::decimal128 x01, std::decimal::decimal128 x02, std::decimal::decimal128 x03, std::decimal::decimal128 x04, std::decimal::decimal128 x05, std::decimal::decimal128 x06, std::decimal::decimal128 x07, std::decimal::decimal128 x08, std::decimal::decimal128 x09, std::decimal::decimal128 x10, std::decimal::decimal128 x11, std::decimal::decimal128 x12, std::decimal::decimal128 x13, std::decimal::decimal128 x14, std::decimal::decimal128 x15, std::decimal::decimal128 x16) { checkd128 (x01, 1); checkd128 (x02, 2); checkd128 (x03, 3); checkd128 (x04, 4); checkd128 (x05, 5); checkd128 (x06, 6); checkd128 (x07, 7); checkd128 (x08, 8); checkd128 (x09, 9); checkd128 (x10, 10); checkd128 (x11, 11); checkd128 (x12, 12); checkd128 (x13, 13); checkd128 (x14, 14); checkd128 (x15, 15); checkd128 (x16, 16); } extern void testvad128 (int n, ...) { int i; va_list ap; if (test_va) { 
# 89 "./compat/decimal/pass_y.h" 3 4
__builtin_va_start(
# 89 "./compat/decimal/pass_y.h"
ap
# 89 "./compat/decimal/pass_y.h" 3 4
,
# 89 "./compat/decimal/pass_y.h"
n
# 89 "./compat/decimal/pass_y.h" 3 4
)
# 89 "./compat/decimal/pass_y.h"
; for (i = 0; i < n; i++) { std::decimal::decimal128 t = 
# 89 "./compat/decimal/pass_y.h" 3 4
__builtin_va_arg(
# 89 "./compat/decimal/pass_y.h"
ap
# 89 "./compat/decimal/pass_y.h" 3 4
,
# 89 "./compat/decimal/pass_y.h"
std::decimal::decimal128
# 89 "./compat/decimal/pass_y.h" 3 4
)
# 89 "./compat/decimal/pass_y.h"
; checkd128 (t, i+1); } 
# 89 "./compat/decimal/pass_y.h" 3 4
__builtin_va_end(
# 89 "./compat/decimal/pass_y.h"
ap
# 89 "./compat/decimal/pass_y.h" 3 4
)
# 89 "./compat/decimal/pass_y.h"
; } }
# 8 "./compat/decimal/pass-2_y.C" 2

//type: fp
//options: 
# 0 "./compat/decimal/pass-4_x.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/decimal/pass-4_x.C"
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
# 2 "./compat/decimal/pass-4_x.C" 2





# 1 "./compat/decimal/pass_x.h" 1
# 1 "./compat/decimal/compat-common.h" 1
# 51 "./compat/decimal/compat-common.h"
extern "C" void abort (void);



extern int fails;
# 2 "./compat/decimal/pass_x.h" 2
# 147 "./compat/decimal/pass_x.h"
std::decimal::decimal32 g01d32, g02d32, g03d32, g04d32; std::decimal::decimal32 g05d32, g06d32, g07d32, g08d32; std::decimal::decimal32 g09d32, g10d32, g11d32, g12d32; std::decimal::decimal32 g13d32, g14d32, g15d32, g16d32; extern void initd32 (std::decimal::decimal32 *p, std::decimal::decimal32 v); extern void checkgd32 (void); extern void testd32 (std::decimal::decimal32 x01, std::decimal::decimal32 x02, std::decimal::decimal32 x03, std::decimal::decimal32 x04, std::decimal::decimal32 x05, std::decimal::decimal32 x06, std::decimal::decimal32 x07, std::decimal::decimal32 x08, std::decimal::decimal32 x09, std::decimal::decimal32 x10, std::decimal::decimal32 x11, std::decimal::decimal32 x12, std::decimal::decimal32 x13, std::decimal::decimal32 x14, std::decimal::decimal32 x15, std::decimal::decimal32 x16); extern void testvad32 (int n, ...); extern void checkd32 (std::decimal::decimal32 x, std::decimal::decimal32 v) { if (x != v + (std::decimal::decimal32)1.5DF) abort (); } extern void test2_d32 (std::decimal::decimal32 x01, std::decimal::decimal32 x02, std::decimal::decimal32 x03, std::decimal::decimal32 x04, std::decimal::decimal32 x05, std::decimal::decimal32 x06, std::decimal::decimal32 x07, std::decimal::decimal32 x08) { testd32 (x01, g02d32, x02, g04d32, x03, g06d32, x04, g08d32, x05, g10d32, x06, g12d32, x07, g14d32, x08, g16d32); } extern void testitd32 (void) { initd32 (&g01d32, 1); initd32 (&g02d32, 2); initd32 (&g03d32, 3); initd32 (&g04d32, 4); initd32 (&g05d32, 5); initd32 (&g06d32, 6); initd32 (&g07d32, 7); initd32 (&g08d32, 8); initd32 (&g09d32, 9); initd32 (&g10d32, 10); initd32 (&g11d32, 11); initd32 (&g12d32, 12); initd32 (&g13d32, 13); initd32 (&g14d32, 14); initd32 (&g15d32, 15); initd32 (&g16d32, 16); checkgd32 (); testd32 (g01d32, g02d32, g03d32, g04d32, g05d32, g06d32, g07d32, g08d32, g09d32, g10d32, g11d32, g12d32, g13d32, g14d32, g15d32, g16d32); testvad32 (1, g01d32); testvad32 (2, g01d32, g02d32); testvad32 (3, g01d32, g02d32, g03d32); testvad32 (4, g01d32, g02d32, g03d32, g04d32); testvad32 (5, g01d32, g02d32, g03d32, g04d32, g05d32); testvad32 (6, g01d32, g02d32, g03d32, g04d32, g05d32, g06d32); testvad32 (7, g01d32, g02d32, g03d32, g04d32, g05d32, g06d32, g07d32); testvad32 (8, g01d32, g02d32, g03d32, g04d32, g05d32, g06d32, g07d32, g08d32); testvad32 (9, g01d32, g02d32, g03d32, g04d32, g05d32, g06d32, g07d32, g08d32, g09d32); testvad32 (10, g01d32, g02d32, g03d32, g04d32, g05d32, g06d32, g07d32, g08d32, g09d32, g10d32); testvad32 (11, g01d32, g02d32, g03d32, g04d32, g05d32, g06d32, g07d32, g08d32, g09d32, g10d32, g11d32); testvad32 (12, g01d32, g02d32, g03d32, g04d32, g05d32, g06d32, g07d32, g08d32, g09d32, g10d32, g11d32, g12d32); testvad32 (13, g01d32, g02d32, g03d32, g04d32, g05d32, g06d32, g07d32, g08d32, g09d32, g10d32, g11d32, g12d32, g13d32); testvad32 (14, g01d32, g02d32, g03d32, g04d32, g05d32, g06d32, g07d32, g08d32, g09d32, g10d32, g11d32, g12d32, g13d32, g14d32); testvad32 (15, g01d32, g02d32, g03d32, g04d32, g05d32, g06d32, g07d32, g08d32, g09d32, g10d32, g11d32, g12d32, g13d32, g14d32, g15d32); testvad32 (16, g01d32, g02d32, g03d32, g04d32, g05d32, g06d32, g07d32, g08d32, g09d32, g10d32, g11d32, g12d32, g13d32, g14d32, g15d32, g16d32); test2_d32 (g01d32, g03d32, g05d32, g07d32, g09d32, g11d32, g13d32, g15d32); }
std::decimal::decimal64 g01d64, g02d64, g03d64, g04d64; std::decimal::decimal64 g05d64, g06d64, g07d64, g08d64; std::decimal::decimal64 g09d64, g10d64, g11d64, g12d64; std::decimal::decimal64 g13d64, g14d64, g15d64, g16d64; extern void initd64 (std::decimal::decimal64 *p, std::decimal::decimal64 v); extern void checkgd64 (void); extern void testd64 (std::decimal::decimal64 x01, std::decimal::decimal64 x02, std::decimal::decimal64 x03, std::decimal::decimal64 x04, std::decimal::decimal64 x05, std::decimal::decimal64 x06, std::decimal::decimal64 x07, std::decimal::decimal64 x08, std::decimal::decimal64 x09, std::decimal::decimal64 x10, std::decimal::decimal64 x11, std::decimal::decimal64 x12, std::decimal::decimal64 x13, std::decimal::decimal64 x14, std::decimal::decimal64 x15, std::decimal::decimal64 x16); extern void testvad64 (int n, ...); extern void checkd64 (std::decimal::decimal64 x, std::decimal::decimal64 v) { if (x != v + (std::decimal::decimal64)2.5DD) abort (); } extern void test2_d64 (std::decimal::decimal64 x01, std::decimal::decimal64 x02, std::decimal::decimal64 x03, std::decimal::decimal64 x04, std::decimal::decimal64 x05, std::decimal::decimal64 x06, std::decimal::decimal64 x07, std::decimal::decimal64 x08) { testd64 (x01, g02d64, x02, g04d64, x03, g06d64, x04, g08d64, x05, g10d64, x06, g12d64, x07, g14d64, x08, g16d64); } extern void testitd64 (void) { initd64 (&g01d64, 1); initd64 (&g02d64, 2); initd64 (&g03d64, 3); initd64 (&g04d64, 4); initd64 (&g05d64, 5); initd64 (&g06d64, 6); initd64 (&g07d64, 7); initd64 (&g08d64, 8); initd64 (&g09d64, 9); initd64 (&g10d64, 10); initd64 (&g11d64, 11); initd64 (&g12d64, 12); initd64 (&g13d64, 13); initd64 (&g14d64, 14); initd64 (&g15d64, 15); initd64 (&g16d64, 16); checkgd64 (); testd64 (g01d64, g02d64, g03d64, g04d64, g05d64, g06d64, g07d64, g08d64, g09d64, g10d64, g11d64, g12d64, g13d64, g14d64, g15d64, g16d64); testvad64 (1, g01d64); testvad64 (2, g01d64, g02d64); testvad64 (3, g01d64, g02d64, g03d64); testvad64 (4, g01d64, g02d64, g03d64, g04d64); testvad64 (5, g01d64, g02d64, g03d64, g04d64, g05d64); testvad64 (6, g01d64, g02d64, g03d64, g04d64, g05d64, g06d64); testvad64 (7, g01d64, g02d64, g03d64, g04d64, g05d64, g06d64, g07d64); testvad64 (8, g01d64, g02d64, g03d64, g04d64, g05d64, g06d64, g07d64, g08d64); testvad64 (9, g01d64, g02d64, g03d64, g04d64, g05d64, g06d64, g07d64, g08d64, g09d64); testvad64 (10, g01d64, g02d64, g03d64, g04d64, g05d64, g06d64, g07d64, g08d64, g09d64, g10d64); testvad64 (11, g01d64, g02d64, g03d64, g04d64, g05d64, g06d64, g07d64, g08d64, g09d64, g10d64, g11d64); testvad64 (12, g01d64, g02d64, g03d64, g04d64, g05d64, g06d64, g07d64, g08d64, g09d64, g10d64, g11d64, g12d64); testvad64 (13, g01d64, g02d64, g03d64, g04d64, g05d64, g06d64, g07d64, g08d64, g09d64, g10d64, g11d64, g12d64, g13d64); testvad64 (14, g01d64, g02d64, g03d64, g04d64, g05d64, g06d64, g07d64, g08d64, g09d64, g10d64, g11d64, g12d64, g13d64, g14d64); testvad64 (15, g01d64, g02d64, g03d64, g04d64, g05d64, g06d64, g07d64, g08d64, g09d64, g10d64, g11d64, g12d64, g13d64, g14d64, g15d64); testvad64 (16, g01d64, g02d64, g03d64, g04d64, g05d64, g06d64, g07d64, g08d64, g09d64, g10d64, g11d64, g12d64, g13d64, g14d64, g15d64, g16d64); test2_d64 (g01d64, g03d64, g05d64, g07d64, g09d64, g11d64, g13d64, g15d64); }
std::decimal::decimal128 g01d128, g02d128, g03d128, g04d128; std::decimal::decimal128 g05d128, g06d128, g07d128, g08d128; std::decimal::decimal128 g09d128, g10d128, g11d128, g12d128; std::decimal::decimal128 g13d128, g14d128, g15d128, g16d128; extern void initd128 (std::decimal::decimal128 *p, std::decimal::decimal128 v); extern void checkgd128 (void); extern void testd128 (std::decimal::decimal128 x01, std::decimal::decimal128 x02, std::decimal::decimal128 x03, std::decimal::decimal128 x04, std::decimal::decimal128 x05, std::decimal::decimal128 x06, std::decimal::decimal128 x07, std::decimal::decimal128 x08, std::decimal::decimal128 x09, std::decimal::decimal128 x10, std::decimal::decimal128 x11, std::decimal::decimal128 x12, std::decimal::decimal128 x13, std::decimal::decimal128 x14, std::decimal::decimal128 x15, std::decimal::decimal128 x16); extern void testvad128 (int n, ...); extern void checkd128 (std::decimal::decimal128 x, std::decimal::decimal128 v) { if (x != v + (std::decimal::decimal128)3.5DL) abort (); } extern void test2_d128 (std::decimal::decimal128 x01, std::decimal::decimal128 x02, std::decimal::decimal128 x03, std::decimal::decimal128 x04, std::decimal::decimal128 x05, std::decimal::decimal128 x06, std::decimal::decimal128 x07, std::decimal::decimal128 x08) { testd128 (x01, g02d128, x02, g04d128, x03, g06d128, x04, g08d128, x05, g10d128, x06, g12d128, x07, g14d128, x08, g16d128); } extern void testitd128 (void) { initd128 (&g01d128, 1); initd128 (&g02d128, 2); initd128 (&g03d128, 3); initd128 (&g04d128, 4); initd128 (&g05d128, 5); initd128 (&g06d128, 6); initd128 (&g07d128, 7); initd128 (&g08d128, 8); initd128 (&g09d128, 9); initd128 (&g10d128, 10); initd128 (&g11d128, 11); initd128 (&g12d128, 12); initd128 (&g13d128, 13); initd128 (&g14d128, 14); initd128 (&g15d128, 15); initd128 (&g16d128, 16); checkgd128 (); testd128 (g01d128, g02d128, g03d128, g04d128, g05d128, g06d128, g07d128, g08d128, g09d128, g10d128, g11d128, g12d128, g13d128, g14d128, g15d128, g16d128); testvad128 (1, g01d128); testvad128 (2, g01d128, g02d128); testvad128 (3, g01d128, g02d128, g03d128); testvad128 (4, g01d128, g02d128, g03d128, g04d128); testvad128 (5, g01d128, g02d128, g03d128, g04d128, g05d128); testvad128 (6, g01d128, g02d128, g03d128, g04d128, g05d128, g06d128); testvad128 (7, g01d128, g02d128, g03d128, g04d128, g05d128, g06d128, g07d128); testvad128 (8, g01d128, g02d128, g03d128, g04d128, g05d128, g06d128, g07d128, g08d128); testvad128 (9, g01d128, g02d128, g03d128, g04d128, g05d128, g06d128, g07d128, g08d128, g09d128); testvad128 (10, g01d128, g02d128, g03d128, g04d128, g05d128, g06d128, g07d128, g08d128, g09d128, g10d128); testvad128 (11, g01d128, g02d128, g03d128, g04d128, g05d128, g06d128, g07d128, g08d128, g09d128, g10d128, g11d128); testvad128 (12, g01d128, g02d128, g03d128, g04d128, g05d128, g06d128, g07d128, g08d128, g09d128, g10d128, g11d128, g12d128); testvad128 (13, g01d128, g02d128, g03d128, g04d128, g05d128, g06d128, g07d128, g08d128, g09d128, g10d128, g11d128, g12d128, g13d128); testvad128 (14, g01d128, g02d128, g03d128, g04d128, g05d128, g06d128, g07d128, g08d128, g09d128, g10d128, g11d128, g12d128, g13d128, g14d128); testvad128 (15, g01d128, g02d128, g03d128, g04d128, g05d128, g06d128, g07d128, g08d128, g09d128, g10d128, g11d128, g12d128, g13d128, g14d128, g15d128); testvad128 (16, g01d128, g02d128, g03d128, g04d128, g05d128, g06d128, g07d128, g08d128, g09d128, g10d128, g11d128, g12d128, g13d128, g14d128, g15d128, g16d128); test2_d128 (g01d128, g03d128, g05d128, g07d128, g09d128, g11d128, g13d128, g15d128); }
# 8 "./compat/decimal/pass-4_x.C" 2

void
pass_4_x (void)
{





testitd32 ();


testitd64 ();


testitd128 ();




if (fails != 0)
  abort ();


}

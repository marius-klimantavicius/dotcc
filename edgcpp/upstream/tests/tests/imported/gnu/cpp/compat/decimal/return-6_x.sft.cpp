//type: fp
//options: 
# 0 "./compat/decimal/return-6_x.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/decimal/return-6_x.C"
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
# 2 "./compat/decimal/return-6_x.C" 2

typedef std::decimal::decimal32 dec32;
typedef std::decimal::decimal64 dec64;
typedef std::decimal::decimal128 dec128;

# 1 "./compat/decimal/return_x.h" 1
# 1 "./compat/decimal/compat-common.h" 1
# 51 "./compat/decimal/compat-common.h"
extern "C" void abort (void);



extern int fails;
# 2 "./compat/decimal/return_x.h" 2




const int test_va = 1;
# 86 "./compat/decimal/return_x.h"
dec32 g01d32, g02d32, g03d32, g04d32; dec32 g05d32, g06d32, g07d32, g08d32; dec32 g09d32, g10d32, g11d32, g12d32; dec32 g13d32, g14d32, g15d32, g16d32; extern void initd32 (dec32 *p, dec32 v); extern void checkgd32 (void); extern dec32 test0d32 (void); extern dec32 test1d32 (dec32); extern dec32 testvad32 (int n, ...); extern void checkd32 (dec32 x, dec32 v) { if (x != v) abort (); } extern void testitd32 (void) { dec32 rslt; initd32 (&g01d32, 1); initd32 (&g02d32, 2); initd32 (&g03d32, 3); initd32 (&g04d32, 4); initd32 (&g05d32, 5); initd32 (&g06d32, 6); initd32 (&g07d32, 7); initd32 (&g08d32, 8); initd32 (&g09d32, 9); initd32 (&g10d32, 10); initd32 (&g11d32, 11); initd32 (&g12d32, 12); initd32 (&g13d32, 13); initd32 (&g14d32, 14); initd32 (&g15d32, 15); initd32 (&g16d32, 16); checkgd32 (); rslt = test0d32 (); checkd32 (rslt, g01d32); rslt = test1d32 (g01d32); checkd32 (rslt, g01d32); if (test_va) { rslt = testvad32 (1, g01d32); checkd32 (rslt, g01d32); rslt = testvad32 (5, g01d32, g02d32, g03d32, g04d32, g05d32); checkd32 (rslt, g05d32); rslt = testvad32 (9, g01d32, g02d32, g03d32, g04d32, g05d32, g06d32, g07d32, g08d32, g09d32); checkd32 (rslt, g09d32); rslt = testvad32 (16, g01d32, g02d32, g03d32, g04d32, g05d32, g06d32, g07d32, g08d32, g09d32, g10d32, g11d32, g12d32, g13d32, g14d32, g15d32, g16d32); checkd32 (rslt, g16d32); } };
dec64 g01d64, g02d64, g03d64, g04d64; dec64 g05d64, g06d64, g07d64, g08d64; dec64 g09d64, g10d64, g11d64, g12d64; dec64 g13d64, g14d64, g15d64, g16d64; extern void initd64 (dec64 *p, dec64 v); extern void checkgd64 (void); extern dec64 test0d64 (void); extern dec64 test1d64 (dec64); extern dec64 testvad64 (int n, ...); extern void checkd64 (dec64 x, dec64 v) { if (x != v) abort (); } extern void testitd64 (void) { dec64 rslt; initd64 (&g01d64, 1); initd64 (&g02d64, 2); initd64 (&g03d64, 3); initd64 (&g04d64, 4); initd64 (&g05d64, 5); initd64 (&g06d64, 6); initd64 (&g07d64, 7); initd64 (&g08d64, 8); initd64 (&g09d64, 9); initd64 (&g10d64, 10); initd64 (&g11d64, 11); initd64 (&g12d64, 12); initd64 (&g13d64, 13); initd64 (&g14d64, 14); initd64 (&g15d64, 15); initd64 (&g16d64, 16); checkgd64 (); rslt = test0d64 (); checkd64 (rslt, g01d64); rslt = test1d64 (g01d64); checkd64 (rslt, g01d64); if (test_va) { rslt = testvad64 (1, g01d64); checkd64 (rslt, g01d64); rslt = testvad64 (5, g01d64, g02d64, g03d64, g04d64, g05d64); checkd64 (rslt, g05d64); rslt = testvad64 (9, g01d64, g02d64, g03d64, g04d64, g05d64, g06d64, g07d64, g08d64, g09d64); checkd64 (rslt, g09d64); rslt = testvad64 (16, g01d64, g02d64, g03d64, g04d64, g05d64, g06d64, g07d64, g08d64, g09d64, g10d64, g11d64, g12d64, g13d64, g14d64, g15d64, g16d64); checkd64 (rslt, g16d64); } };
dec128 g01d128, g02d128, g03d128, g04d128; dec128 g05d128, g06d128, g07d128, g08d128; dec128 g09d128, g10d128, g11d128, g12d128; dec128 g13d128, g14d128, g15d128, g16d128; extern void initd128 (dec128 *p, dec128 v); extern void checkgd128 (void); extern dec128 test0d128 (void); extern dec128 test1d128 (dec128); extern dec128 testvad128 (int n, ...); extern void checkd128 (dec128 x, dec128 v) { if (x != v) abort (); } extern void testitd128 (void) { dec128 rslt; initd128 (&g01d128, 1); initd128 (&g02d128, 2); initd128 (&g03d128, 3); initd128 (&g04d128, 4); initd128 (&g05d128, 5); initd128 (&g06d128, 6); initd128 (&g07d128, 7); initd128 (&g08d128, 8); initd128 (&g09d128, 9); initd128 (&g10d128, 10); initd128 (&g11d128, 11); initd128 (&g12d128, 12); initd128 (&g13d128, 13); initd128 (&g14d128, 14); initd128 (&g15d128, 15); initd128 (&g16d128, 16); checkgd128 (); rslt = test0d128 (); checkd128 (rslt, g01d128); rslt = test1d128 (g01d128); checkd128 (rslt, g01d128); if (test_va) { rslt = testvad128 (1, g01d128); checkd128 (rslt, g01d128); rslt = testvad128 (5, g01d128, g02d128, g03d128, g04d128, g05d128); checkd128 (rslt, g05d128); rslt = testvad128 (9, g01d128, g02d128, g03d128, g04d128, g05d128, g06d128, g07d128, g08d128, g09d128); checkd128 (rslt, g09d128); rslt = testvad128 (16, g01d128, g02d128, g03d128, g04d128, g05d128, g06d128, g07d128, g08d128, g09d128, g10d128, g11d128, g12d128, g13d128, g14d128, g15d128, g16d128); checkd128 (rslt, g16d128); } };
# 8 "./compat/decimal/return-6_x.C" 2

void
return_6_x (void)
{




testitd32 ();
testitd64 ();
testitd128 ();



if (fails != 0)
  abort ();


}

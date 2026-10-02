//type: fp
//options: 
# 0 "./Wstringop-overflow-43.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wstringop-overflow-43.c"




# 1 "./range.h" 1
# 11 "./range.h"
typedef int int32_t;
typedef long int ptrdiff_t;
typedef long unsigned int size_t;

static inline ptrdiff_t signed_value (void)
{
  extern volatile ptrdiff_t signed_value_source;
  return signed_value_source;
}

static inline size_t unsigned_value (void)
{
  extern volatile size_t unsigned_value_source;
  return unsigned_value_source;
}

static inline ptrdiff_t signed_range (ptrdiff_t min, ptrdiff_t max)
{
  ptrdiff_t val = signed_value ();
  return val < min || max < val ? min : val;
}

static inline ptrdiff_t signed_anti_range (ptrdiff_t min, ptrdiff_t max)
{
  ptrdiff_t val = signed_value ();
  return min <= val && val <= max ? min == (-0x7fffffffffffffffL - 1) ? max + 1 : min - 1 : val;
}

static inline size_t unsigned_range (size_t min, size_t max)
{
  size_t val = unsigned_value ();
  return val < min || max < val ? min : val;
}

static inline size_t unsigned_anti_range (size_t min, size_t max)
{
  size_t val = unsigned_value ();
  return min <= val && val <= max ? min == 0 ? max + 1 : min - 1 : val;
}
# 6 "./Wstringop-overflow-43.c" 2





typedef long unsigned int size_t;

void* memset (void *, int, size_t);

void sink (void*, ...);

extern char a11[11];
struct S { char a11[11], b; };
extern struct S sa11;
# 32 "./Wstringop-overflow-43.c"
void nowarn_memset_array_cst (void)
{
  char *p = &a11[11];

  do { char *_p0 = p; char *_p1 = _p0 + (-11); char *_p2 = _p1 + (0); memset (_p2, 0, 11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (-10); char *_p2 = _p1 + (0); memset (_p2, 0, 10); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (-9); char *_p2 = _p1 + (0); memset (_p2, 0, 9); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (-8); char *_p2 = _p1 + (0); memset (_p2, 0, 8); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (-3); char *_p2 = _p1 + (0); memset (_p2, 0, 3); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (-2); char *_p2 = _p1 + (0); memset (_p2, 0, 2); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (-1); char *_p2 = _p1 + (0); memset (_p2, 0, 1); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (0); char *_p2 = _p1 + (0); memset (_p2, 0, 0); sink (p, _p0, _p1, _p2); } while (0);;

  do { char *_p0 = p; char *_p1 = _p0 + (-6); char *_p2 = _p1 + (-5); memset (_p2, 0, 11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (-6); char *_p2 = _p1 + (-4); memset (_p2, 0, 10); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (-6); char *_p2 = _p1 + (-3); memset (_p2, 0, 9); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (-6); char *_p2 = _p1 + (-2); memset (_p2, 0, 8); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (-6); char *_p2 = _p1 + (-1); memset (_p2, 0, 7); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (-5); char *_p2 = _p1 + (-6); memset (_p2, 0, 11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (-5); char *_p2 = _p1 + (-5); memset (_p2, 0, 10); sink (p, _p0, _p1, _p2); } while (0);;
}

void nowarn_memset_array_rng_int (void)
{
  char *p = &a11[11];

  int i11 = signed_range ((11), (0x7fffffff));
  int i10 = signed_range ((10), (0x7fffffff));
  int i9 = signed_range ((9), (0x7fffffff));
  int i3 = signed_range ((3), (0x7fffffff));
  int i2 = signed_range ((2), (0x7fffffff));
  int i1 = signed_range ((1), (0x7fffffff));
  int i0 = signed_range ((0), (0x7fffffff));

  int m11 = signed_range ((-(0x7fffffff - 1)), (-11));
  int m10 = signed_range ((-(0x7fffffff - 1)), (-10));
  int m9 = signed_range ((-(0x7fffffff - 1)), (-9));
  int m3 = signed_range ((-(0x7fffffff - 1)), (-3));
  int m2 = signed_range ((-(0x7fffffff - 1)), (-2));
  int m1 = signed_range ((-(0x7fffffff - 1)), (-1));
  int m0 = signed_range ((-(0x7fffffff - 1)), (-0));

  do { char *_p0 = p; char *_p1 = _p0 + (m11); char *_p2 = _p1 + (0); memset (_p2, 0, i11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (m10); char *_p2 = _p1 + (0); memset (_p2, 0, i10); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (m9); char *_p2 = _p1 + (0); memset (_p2, 0, i9); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (m3); char *_p2 = _p1 + (0); memset (_p2, 0, i3); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (m2); char *_p2 = _p1 + (0); memset (_p2, 0, i2); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (m1); char *_p2 = _p1 + (0); memset (_p2, 0, i1); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (m0); char *_p2 = _p1 + (0); memset (_p2, 0, i0); sink (p, _p0, _p1, _p2); } while (0);;

  do { char *_p0 = p; char *_p1 = _p0 + (m11); char *_p2 = _p1 + (0); memset (_p2, 0, i11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (m10); char *_p2 = _p1 + (0); memset (_p2, 0, i10); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (m9); char *_p2 = _p1 + (0); memset (_p2, 0, i9); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (m3); char *_p2 = _p1 + (0); memset (_p2, 0, i3); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (m2); char *_p2 = _p1 + (0); memset (_p2, 0, i2); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (m1); char *_p2 = _p1 + (0); memset (_p2, 0, i1); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (m0); char *_p2 = _p1 + (0); memset (_p2, 0, i0); sink (p, _p0, _p1, _p2); } while (0);;
}


void nowarn_memset_array_rng (void)
{
  char *p = &a11[11];

  do { char *_p0 = p; char *_p1 = _p0 + (signed_range ((-11), (-10))); char *_p2 = _p1 + (signed_range ((-2), (-1))); memset (_p2, 0, unsigned_range ((11), (12))); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_range ((-10), (-9))); char *_p2 = _p1 + (signed_range ((-1), (0))); memset (_p2, 0, unsigned_range ((11), (13))); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_range ((-9), (-8))); char *_p2 = _p1 + (signed_range ((-2), (-1))); memset (_p2, 0, unsigned_range ((11), (14))); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_range ((-8), (-7))); char *_p2 = _p1 + (signed_range ((-3), (-2))); memset (_p2, 0, unsigned_range ((11), (15))); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_range ((-7), (-6))); char *_p2 = _p1 + (signed_range ((-4), (-3))); memset (_p2, 0, unsigned_range ((11), (16))); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_range ((-6), (-5))); char *_p2 = _p1 + (signed_range ((-5), (-4))); memset (_p2, 0, unsigned_range ((11), (17))); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_range ((-5), (-4))); char *_p2 = _p1 + (signed_range ((-6), (-5))); memset (_p2, 0, unsigned_range ((11), (18))); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_range ((-4), (-3))); char *_p2 = _p1 + (signed_range ((-7), (-6))); memset (_p2, 0, unsigned_range ((11), (19))); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_range ((-3), (-2))); char *_p2 = _p1 + (signed_range ((-8), (-7))); memset (_p2, 0, unsigned_range ((11), (0x7fffffff))); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_range ((-2), (-1))); char *_p2 = _p1 + (signed_range ((-9), (-8))); memset (_p2, 0, unsigned_range ((11), ((2U * 0x7fffffff + 1)))); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_range ((-1), (0))); char *_p2 = _p1 + (signed_range ((-10), (-9))); memset (_p2, 0, unsigned_range ((11), (0x7fffffffffffffffL))); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_range ((0), (1))); char *_p2 = _p1 + (signed_range ((-11), (-10))); memset (_p2, 0, unsigned_range ((11), (0xffffffffffffffffUL))); sink (p, _p0, _p1, _p2); } while (0);;

  do { char *_p0 = p; char *_p1 = _p0 + (signed_range (((-0x7fffffffffffffffL - 1)), (-10))); char *_p2 = _p1 + (signed_range (((-0x7fffffffffffffffL - 1)), (-1))); memset (_p2, 0, unsigned_range ((10), (12))); sink (p, _p0, _p1, _p2); } while (0);;

  do { char *_p0 = p; char *_p1 = _p0 + (signed_range ((-11), (-10))); char *_p2 = _p1 + (signed_range ((-3), (-1))); memset (_p2, 0, unsigned_range ((10), (12))); sink (p, _p0, _p1, _p2); } while (0);
  do { char *_p0 = p; char *_p1 = _p0 + (signed_range ((-11), (-10))); char *_p2 = _p1 + (signed_range ((-3), (-1))); memset (_p2, 0, unsigned_range ((10), (12))); sink (p, _p0, _p1, _p2); } while (0);
}


void warn_memset_array_rng (void)
{
  char *p = &a11[11];
  size_t n11_12 = unsigned_range ((11), (12));
  size_t n10_12 = unsigned_range ((10), (12));

  do { char *_p0 = p; char *_p1 = _p0 + (signed_range ((-11), (-10))); char *_p2 = _p1 + (signed_range ((-3), (-2))); memset (_p2, 0, n11_12); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_range ((-11), (-10))); char *_p2 = _p1 + (signed_range ((-3), (-2))); memset (_p2, 0, n10_12); sink (p, _p0, _p1, _p2); } while (0);;
}


void nowarn_memset_anti_range (void)
{
  size_t n11 = unsigned_range ((11), (0xffffffffffffffffUL));

  char *p = &a11[11];

  do { char *_p0 = p; char *_p1 = _p0 + ((int)signed_anti_range ((-(0x7fffffff - 1)), (-12))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + ((int)signed_anti_range ((-13), (-13))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + ((int)signed_anti_range ((-13), (-12))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + ((int)signed_anti_range ((-10), (1))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + ((int)signed_anti_range ((-10), (11))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + ((int)signed_anti_range ((-10), (0x7fffffff))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + ((int)signed_anti_range ((-1), (-1))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + ((int)signed_anti_range ((-1), (0))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + ((int)signed_anti_range ((-1), (11))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + ((int)signed_anti_range ((-1), (0x7fffffff))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;

  do { char *_p0 = p; char *_p1 = _p0 + (signed_anti_range (((-0x7fffffffffffffffL - 1)), (-12))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_anti_range ((-13), (-13))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_anti_range ((-13), (-12))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_anti_range ((-10), (1))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_anti_range ((-10), (11))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_anti_range ((-10), (0x7fffffffffffffffL))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_anti_range ((-1), (-1))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_anti_range ((-1), (0))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_anti_range ((-1), (11))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_anti_range ((-1), (0x7fffffffffffffffL))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
}

void warn_memset_reversed_range (void)
{
  size_t n11 = unsigned_range ((11), (0xffffffffffffffffUL));

  char *p = &a11[11];







  do { char *_p0 = p; char *_p1 = _p0 + (signed_anti_range ((-(0x7fffffff - 1)), (-11))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;



  do { char *_p0 = p; char *_p1 = _p0 + (signed_anti_range ((-(0x7fffffff - 1)), (11))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_anti_range ((-(0x7fffffff - 1)), (1))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_anti_range ((-(0x7fffffff - 1)), (0))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;

  do { char *_p0 = p; char *_p1 = _p0 + (signed_anti_range ((-12), (-11))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_anti_range ((-12), (-1))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_anti_range ((-11), (0))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
  do { char *_p0 = p; char *_p1 = _p0 + (signed_anti_range ((-11), (11))); char *_p2 = _p1 + (0); memset (_p2, 0, n11); sink (p, _p0, _p1, _p2); } while (0);;
}

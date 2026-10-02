//type: fp
//options: 
# 0 "./warn-sprintf-no-nul.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn-sprintf-no-nul.c"






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
# 8 "./warn-sprintf-no-nul.c" 2

typedef int wchar_t;

extern int sprintf (char*, const char*, ...);

extern char *dst;

int i0 = 0;
int i1 = 1;

void sink (int, ...);







const char a[5] = "12345";
const char b[6] = "123456";
const char a2[][3] = {
  "", "1", "12", "123", "123\000"
};


void test_narrow (void)
{


  sink (sprintf (dst, "%.0s%.1s%.2s%.3s%.4s%.5s", a, a, a, a, a, a));

  sink (sprintf (dst, "%s", a));
  sink (sprintf (dst, "%.6s", a));


  const char *s0 = i0 < 0 ? a2[0] : a2[3];
  sink (sprintf (dst, "%s", s0));
  s0 = i0 < 0 ? "123456" : a2[4];
  sink (sprintf (dst, "%s", s0));

  const char *s1 = i0 < 0 ? a2[3] : a2[0];
  sink (sprintf (dst, "%s", s1));

  const char *s2 = i0 < 0 ? a2[3] : a2[4];
  sink (sprintf (dst, "%s", s2));

  s0 = i0 < 0 ? a : b;
  sink (sprintf (dst, "%.5s", s0));




  sink (sprintf (dst, "%.6s", s0));

  s0 = i0 < 0 ? b : a;
  sink (sprintf (dst, "%.7s", s0));



  int r = signed_range ((4), (5));

  sink (sprintf (dst, "%.*s", r, a));
  sink (sprintf (dst, "%.*s", r, b));

  r = signed_range ((5), (6));
  sink (sprintf (dst, "%.*s", r, a));
  sink (sprintf (dst, "%.*s", r, b));

  r = signed_range ((6), (7));
  sink (sprintf (dst, "%.*s", r, a));
  sink (sprintf (dst, "%.*s", r, b));
}


const wchar_t wa[5] = L"12345";

void test_wide (void)
{
  sink (sprintf (dst, "%.0ls%.1ls%.2ls%.3ls%.4ls%.5ls", wa, wa, wa, wa, wa, wa));

  sink (sprintf (dst, "%ls", wa));
  sink (sprintf (dst, "%.6ls", wa));
}

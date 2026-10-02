//type: fp
//options: 
# 0 "./Wrestrict-6.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wrestrict-6.c"
# 9 "./Wrestrict-6.c"
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
# 10 "./Wrestrict-6.c" 2

extern char* strcpy (char*, const char*);
extern char* stpcpy (char*, const char*);

void sink (void*);

void warn_2_smax_p2 (void)
{
  char a[7] = "01234";

  char *d = a;

  ptrdiff_t i = unsigned_range ((2), (0x7fffffffffffffffL + (size_t)2));

  strcpy (d, d + i);

  sink (d);
}

void nowarn_3_smax_p2 (void)
{
  char a[7] = "12345";

  char *d = a;

  ptrdiff_t i = unsigned_range ((3), (0x7fffffffffffffffL + (size_t)2));

  strcpy (d, d + i);

  sink (d);
}

void warn_2u_smax_p2 (void)
{
  char a[7] = "23456";

  char *d = a;

  size_t i = unsigned_range ((2), (0x7fffffffffffffffL + (size_t)2));

  strcpy (d, d + i);

  sink (d);
}

void nowarn_3u_smax_p2 (void)
{
  char a[7] = "34567";

  char *d = a;

  size_t i = unsigned_range ((3), (0x7fffffffffffffffL + (size_t)2));

  strcpy (d, d + i);

  sink (d);
}

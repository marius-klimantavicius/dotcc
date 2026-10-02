//type: fp
//options: 
# 0 "./Wstringop-overflow-46.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wstringop-overflow-46.c"






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
# 8 "./Wstringop-overflow-46.c" 2

void* malloc (size_t);
void* memchr (void*, int, size_t);
void* memset (void*, int, size_t);

void sink (void*, ...);

void nowarn_memchr_cst_memset_cst (const void *s)
{
  char *p = malloc (4);
  sink (p);

  p = memchr (p, '1', 4);
  memset (p, 0, 4);
}

void nowarn_memchr_uint_memset_cst (const void *s, unsigned n)
{
  char *p = malloc (4);
  sink (p);

  p = memchr (p, '1', n);
  memset (p, 0, 4);
}

void nowarn_memchr_sz_memset_cst (const void *s, size_t n)
{
  char *p = malloc (4);
  sink (p);

  p = memchr (p, '1', n);
  memset (p, 0, 4);
}

void nowarn_memchr_anti_range_memset_cst (const void *s, size_t n)
{
  char *p = malloc (4);
  sink (p);

  if (n == 0)
    n = 1;

  p = memchr (p, '1', n);
  memset (p, 0, 4);
}

void warn_memchr_cst_memset_cst (const void *s)
{
  char *p = malloc (4);
  sink (p);

  p = memchr (p, '1', 4);
  memset (p, 0, 5);
}

void warn_memchr_var_memset_cst (const void *s, unsigned n)
{
  char *p = malloc (4);
  sink (p);

  p = memchr (p, '1', n);
  memset (p, 0, 5);
}

void warn_memchr_var_memset_range (const void *s, unsigned n)
{







  char *p0 = malloc (unsigned_range ((5), (7)));



  sink (p0);
  char *p1 = memchr (p0, '1', n);
  memset (p1, 0, unsigned_range ((8), (9)));

  sink (p0);
  p1 = memchr (p0 + 1, '2', n);
  memset (p1, 0, unsigned_range ((7), (9)));

  sink (p0);
  char *p2 = memchr (p1 + 1, '3', n);
  memset (p2, 0, unsigned_range ((6), (9)));
}

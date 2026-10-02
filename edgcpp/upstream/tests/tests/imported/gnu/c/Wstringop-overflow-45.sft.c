//type: fp
//options: 
# 0 "./Wstringop-overflow-45.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wstringop-overflow-45.c"






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
# 8 "./Wstringop-overflow-45.c" 2

void* malloc (size_t);
void* memcpy (void*, const void*, size_t);
void* memmove (void*, const void*, size_t);
void* mempcpy (void*, const void*, size_t);

void sink (void*, ...);


void nowarn_memcpy (const void *s)
{
  extern char cpy_a4[4];
  unsigned n = sizeof cpy_a4;

  void *p = cpy_a4;
  p = memcpy (p, s, n);
  sink (p);
  memcpy (p, s, n);
  sink (p);

  p = cpy_a4 + 1;
  p = memcpy (p, s, n - 1);
  sink (p);
  memcpy (p, s, n - 1);
  sink (p);

  p = cpy_a4 + 2;
  p = memcpy (p, s, n - 2);
  sink (p);
  memcpy (p, s, n - 2);
  sink (p);

  p = cpy_a4 + 3;
  p = memcpy (p, s, n - 3);
  sink (p);
  memcpy (p, s, n - 3);
  sink (p);

  p = cpy_a4 + 4;
  p = memcpy (p, s, n - 4);
  sink (p);
  memcpy (p, s, n - 4);
  sink (p);
}


void nowarn_memcpy_chain (const void *s)
{
  extern char cpy_a8[8];

  char *p = cpy_a8;

  p = memcpy (p + 1, s, 7);
  sink (p);

  p = memcpy (p + 2 , s, 5);
  sink (p);

  p = memcpy (p + 3 , s, 2);
  sink (p);

  p = memcpy (p + 1 , s, 1);
  sink (p);

  p = memcpy (p - 7 , s, 8);
  sink (p);

  memcpy (p + 1, s, 7);
}


void warn_memcpy (const void *s)
{
  extern char cpy_a5[5];

  unsigned n = sizeof cpy_a5;
  void *p = cpy_a5;

  p = memcpy (p, s, n);
  sink (p);
  memcpy (p, s, n + 1);
  sink (p);

  p = cpy_a5;
  p = memcpy (p, s, n);
  sink (p);
  memcpy (p, s, n + 1);
  sink (p);

  p = cpy_a5 + 1;
  p = memcpy (p, s, n - 1);
  sink (p);
  memcpy (p, s, n);
  sink (p);
}


void warn_memcpy_chain (const void *s)
{
  extern char cpy_a8[8];

  char *p = cpy_a8;

  p = memcpy (p, s, 9);
  sink (p);

  p = memcpy (p + 2, s, 7);
  sink (p);

  p = memcpy (p + 3, s, 5);
  sink (p);

  p = memcpy (p + 3, s, 3);
  sink (p);
}


void nowarn_mempcpy (const void *s)
{
  extern char a4[4];
  unsigned n = sizeof a4;

  char *p = mempcpy (a4, s, n);
  sink (p);
  mempcpy (p - 4, s, n);
  sink (p);

  p = mempcpy (a4 + 1, s, n - 1);
  sink (p);
  mempcpy (p - 4, s, n);
  sink (p);

  p = mempcpy (a4 + 2, s, n - 2);
  sink (p);
  mempcpy (p - 4, s, n);
  sink (p);

  p = mempcpy (a4 + 3, s, n - 3);
  sink (p);
  mempcpy (p - 4, s, n);
  sink (p);

  p = mempcpy (a4 + 4, s, n - 4);
  sink (p);
  mempcpy (p - 4, s, n);
  sink (p);
}


void nowarn_mempcpy_chain (const void *s)
{
  extern char pcpy_a8[8];

  char *p = pcpy_a8;

  p = mempcpy (p + 1, s, 7);
  sink (p);

  p = mempcpy (p - 7 , s, 7);
  sink (p);

  p = mempcpy (p - 5 , s, 5);
  sink (p);

  p = mempcpy (p - 3 , s, 3);
  sink (p);

  p = mempcpy (p - 2 , s, 2);
  sink (p);

  mempcpy (p - 1, s, 1);
  sink (p);

  mempcpy (p - 8, s, 8);
}


void warn_mempcpy (const void *s)
{
  extern char pcpy_a5[5];

  char *p = pcpy_a5;

  p = mempcpy (p, s, 5);
  sink (p);
  mempcpy (p - 5, s, 6);
  sink (p);

  p = pcpy_a5;
  p = mempcpy (p, s, 3);
  sink (p);
  mempcpy (p, s, 3);
  sink (p);

  p = pcpy_a5 + 1;
  p = mempcpy (p, s, 3);
  sink (p);
  mempcpy (p - 1, s, 5);
  sink (p);
}


void warn_mempcpy_chain_3 (const void *s)
{
  char *p = malloc (5);
  p = mempcpy (p, s, unsigned_range ((1), (2)));
  p = mempcpy (p, s, unsigned_range ((2), (3)));
  p = mempcpy (p, s, unsigned_range ((3), (4)));

  sink (p);
}

void warn_mempcpy_offrng_chain_3 (const void *s)
{
  char *p = malloc (11);
  size_t r1_2 = unsigned_range ((1), (2));
  size_t r2_3 = r1_2 + 1;
  size_t r3_4 = r2_3 + 1;

  p = mempcpy (p + r1_2, s, r1_2);
  p = mempcpy (p + r2_3, s, r2_3);
  p = mempcpy (p + r3_4, s, r3_4);

  sink (p);
}

void warn_mempcpy_chain_4 (const void *s)
{
  char *p = malloc (9);
  p = mempcpy (p, s, unsigned_range ((1), (2)));
  p = mempcpy (p, s, unsigned_range ((2), (3)));
  p = mempcpy (p, s, unsigned_range ((3), (4)));
  p = mempcpy (p, s, unsigned_range ((4), (5)));

  sink (p);
}

void warn_mempcpy_chain_5 (const void *s)
{
  char *p = malloc (14);
  p = mempcpy (p, s, unsigned_range ((1), (2)));
  p = mempcpy (p, s, unsigned_range ((2), (3)));
  p = mempcpy (p, s, unsigned_range ((3), (4)));
  p = mempcpy (p, s, unsigned_range ((4), (5)));
  p = mempcpy (p, s, unsigned_range ((5), (6)));

  sink (p);
}

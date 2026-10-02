//type: fp
//options: 
# 0 "./Wstringop-overflow-25.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wstringop-overflow-25.c"




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
# 6 "./Wstringop-overflow-25.c" 2







extern void* alloca (size_t);
extern void* calloc (size_t, size_t);
extern void* malloc (size_t);

extern __attribute__ ((alloc_size (1), malloc)) void*
  alloc1 (size_t, int);
extern __attribute__ ((alloc_size (2), malloc)) void*
  alloc2 (int, size_t);
extern __attribute__ ((alloc_size (2, 4), malloc)) void*
  alloc2_4 (int, size_t, int, size_t);

extern char* strcpy (char*, const char*);

void sink (void*);
# 39 "./Wstringop-overflow-25.c"
__attribute__ ((noipa)) void test_strcpy_alloca (size_t n)
{
  size_t r_0_1 = unsigned_range ((0), (1));
  size_t r_1_2 = unsigned_range ((1), (2));
  size_t r_2_3 = unsigned_range ((2), (3));

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = alloca (r_0_1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = alloca (r_0_1); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = alloca (r_1_2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = alloca (r_1_2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloca (r_1_2); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = alloca (r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloca (r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = alloca (r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = alloca (r_2_3); strcpy (d, s); sink (d); } while (0);

  size_t r_2_smax = unsigned_range ((2), (0xffffffffffffffffUL));
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = alloca (r_2_smax); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = alloca (r_2_smax); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloca (r_2_smax); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = alloca (r_2_smax * 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 4 - 1); char *d = alloca (r_2_smax * 2 + 1); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = alloca (n); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloca (n + 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = alloca (n * 2 + 1); strcpy (d, s); sink (d); } while (0);

  int r_imin_imax = signed_range (((-0x7fffffff - 1)), (0x7fffffff));
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = alloca (r_imin_imax); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloca (r_imin_imax + 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = alloca (r_imin_imax * 2 + 1); strcpy (d, s); sink (d); } while (0);

  int r_0_imax = signed_range ((0), (0x7fffffff));
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = alloca (r_0_imax); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloca (r_0_imax + 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = alloca (r_0_imax * 2 + 1); strcpy (d, s); sink (d); } while (0);

  int r_1_imax = signed_range ((1), (0x7fffffff));
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = alloca (r_1_imax); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloca (r_1_imax + 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = alloca (r_1_imax * 2 + 1); strcpy (d, s); sink (d); } while (0);

  ptrdiff_t r_dmin_dmax = signed_range (((-0x7fffffffffffffffL - 1)), (0x7fffffffffffffffL));
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = alloca (r_dmin_dmax); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloca (r_dmin_dmax + 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = alloca (r_dmin_dmax * 2 + 1); strcpy (d, s); sink (d); } while (0);
}

__attribute__ ((noipa)) void test_strcpy_calloc (void)
{
  size_t r_1_2 = unsigned_range ((1), (2));
  size_t r_2_3 = unsigned_range ((2), (3));

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = calloc (r_1_2, 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = calloc (r_1_2, 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = calloc (r_1_2, 1); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = calloc (r_2_3, 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = calloc (r_2_3, 1); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = calloc (1, r_1_2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = calloc (1, r_1_2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = calloc (1, r_1_2); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = calloc (1, r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = calloc (1, r_2_3); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = calloc (r_1_2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = calloc (r_1_2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = calloc (r_1_2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = calloc (r_1_2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 4 - 1); char *d = calloc (r_1_2, 2); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = calloc (r_2_3, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = calloc (r_2_3, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = calloc (r_2_3, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 5 - 1); char *d = calloc (r_2_3, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 6 - 1); char *d = calloc (r_2_3, 2); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = calloc (r_1_2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = calloc (r_1_2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = calloc (r_1_2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = calloc (r_1_2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 4 - 1); char *d = calloc (r_1_2, 2); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = calloc (r_2_3, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = calloc (r_2_3, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = calloc (r_2_3, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 5 - 1); char *d = calloc (r_2_3, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 6 - 1); char *d = calloc (r_2_3, 2); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = calloc (r_1_2, r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = calloc (r_1_2, r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = calloc (r_1_2, r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = calloc (r_1_2, r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 4 - 1); char *d = calloc (r_1_2, r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 5 - 1); char *d = calloc (r_1_2, r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 6 - 1); char *d = calloc (r_1_2, r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = calloc (r_1_2, r_2_3); strcpy (d, s); sink (d); } while (0);

  size_t r_2_dmax = unsigned_range ((2), (0x7fffffffffffffffL));
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = calloc (0, r_2_dmax); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = calloc (1, r_2_dmax); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = calloc (2, r_2_dmax); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = calloc (r_2_dmax, r_2_dmax); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = calloc (r_2_dmax, r_2_dmax); strcpy (d, s); sink (d); } while (0);

  size_t r_2_smax = unsigned_range ((2), (0xffffffffffffffffUL));
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = calloc (r_2_smax, 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = calloc (r_2_smax, 2); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = calloc (r_2_smax, r_2_smax); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = calloc (r_2_smax, r_2_smax); strcpy (d, s); sink (d); } while (0);
}


__attribute__ ((noipa)) void test_strcpy_malloc (void)
{
  size_t r_0_1 = unsigned_range ((0), (1));
  size_t r_1_2 = unsigned_range ((1), (2));
  size_t r_2_3 = unsigned_range ((2), (3));

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = malloc (r_0_1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = malloc (r_0_1); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = malloc (r_1_2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = malloc (r_1_2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = malloc (r_1_2); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = malloc (r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = malloc (r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = malloc (r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = malloc (r_2_3); strcpy (d, s); sink (d); } while (0);
}


__attribute__ ((noipa)) void test_strcpy_alloc1 (void)
{
  size_t r_0_1 = unsigned_range ((0), (1));
  size_t r_1_2 = unsigned_range ((1), (2));
  size_t r_2_3 = unsigned_range ((2), (3));



  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = alloc1 (r_0_1, 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = alloc1 (r_0_1, 1); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = alloc1 (r_1_2, 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = alloc1 (r_1_2, 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloc1 (r_1_2, 1); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = alloc1 (r_2_3, 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloc1 (r_2_3, 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = alloc1 (r_2_3, 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = alloc1 (r_2_3, 1); strcpy (d, s); sink (d); } while (0);
}

__attribute__ ((noipa)) void test_strcpy_alloc2 (void)
{
  size_t r_0_1 = unsigned_range ((0), (1));
  size_t r_1_2 = unsigned_range ((1), (2));
  size_t r_2_3 = unsigned_range ((2), (3));



  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = alloc1 (r_0_1, 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = alloc1 (r_0_1, 1); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = alloc1 (r_1_2, 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = alloc1 (r_1_2, 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloc1 (r_1_2, 1); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = alloc1 (r_2_3, 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloc1 (r_2_3, 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = alloc1 (r_2_3, 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = alloc1 (r_2_3, 1); strcpy (d, s); sink (d); } while (0);
}


__attribute__ ((noipa)) void test_strcpy_alloc2_4 (void)
{
  size_t r_1_2 = unsigned_range ((1), (2));
  size_t r_2_3 = unsigned_range ((2), (3));



  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = alloc2_4 (1, r_1_2, 2, 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = alloc2_4 (1, r_1_2, 2, 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloc2_4 (1, r_1_2, 2, 1); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloc2_4 (1, r_2_3, 2, 1); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = alloc2_4 (1, r_2_3, 2, 1); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = alloc2_4 (1, 1, 2, r_1_2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = alloc2_4 (1, 1, 2, r_1_2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloc2_4 (1, 1, 2, r_1_2); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloc2_4 (1, 1, 2, r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = alloc2_4 (1, 1, 2, r_2_3); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = alloc2_4 (1, r_1_2, 2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = alloc2_4 (1, r_1_2, 2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloc2_4 (1, r_1_2, 2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = alloc2_4 (1, r_1_2, 2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 4 - 1); char *d = alloc2_4 (1, r_1_2, 2, 2); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = alloc2_4 (1, r_2_3, 2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = alloc2_4 (1, r_2_3, 2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloc2_4 (1, r_2_3, 2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 5 - 1); char *d = alloc2_4 (1, r_2_3, 2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 6 - 1); char *d = alloc2_4 (1, r_2_3, 2, 2); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = alloc2_4 (1, r_1_2, 2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = alloc2_4 (1, r_1_2, 2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloc2_4 (1, r_1_2, 2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = alloc2_4 (1, r_1_2, 2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 4 - 1); char *d = alloc2_4 (1, r_1_2, 2, 2); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = alloc2_4 (1, r_2_3, 2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = alloc2_4 (1, r_2_3, 2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloc2_4 (1, r_2_3, 2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 5 - 1); char *d = alloc2_4 (1, r_2_3, 2, 2); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 6 - 1); char *d = alloc2_4 (1, r_2_3, 2, 2); strcpy (d, s); sink (d); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = alloc2_4 (1, r_1_2, 2, r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = alloc2_4 (1, r_1_2, 2, r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = alloc2_4 (1, r_1_2, 2, r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = alloc2_4 (1, r_1_2, 2, r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 4 - 1); char *d = alloc2_4 (1, r_1_2, 2, r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 5 - 1); char *d = alloc2_4 (1, r_1_2, 2, r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 6 - 1); char *d = alloc2_4 (1, r_1_2, 2, r_2_3); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = alloc2_4 (1, r_1_2, 2, r_2_3); strcpy (d, s); sink (d); } while (0);

  size_t r_2_dmax = unsigned_range ((2), (0x7fffffffffffffffL));
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = alloc2_4 (1, r_2_dmax, 2, r_2_dmax); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = alloc2_4 (1, r_2_dmax, 2, r_2_dmax); strcpy (d, s); sink (d); } while (0);

  size_t r_2_smax = unsigned_range ((2), (0xffffffffffffffffUL));
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = alloc2_4 (1, r_2_smax, 2, r_2_smax); strcpy (d, s); sink (d); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = alloc2_4 (1, r_2_smax, 2, r_2_smax); strcpy (d, s); sink (d); } while (0);
}
# 297 "./Wstringop-overflow-25.c"
__attribute__ ((noipa)) void test_strcpy_vla (const size_t vals[])
{
  size_t idx = 0;

  size_t r_0_1 = (++idx, (vals[idx] < 0 || 1 < vals[idx] ? 0 : vals[idx]));
  size_t r_1_2 = (++idx, (vals[idx] < 1 || 2 < vals[idx] ? 1 : vals[idx]));
  size_t r_2_3 = (++idx, (vals[idx] < 2 || 3 < vals[idx] ? 2 : vals[idx]));

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char vla[r_0_1]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char vla[r_0_1]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char vla[r_1_2]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char vla[r_1_2]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char vla[r_1_2]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char vla[r_2_3]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char vla[r_2_3]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char vla[r_2_3]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char vla[r_2_3]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);


  typedef short int int16_t;

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); int16_t vla[r_1_2]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); int16_t vla[r_1_2]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); int16_t vla[r_1_2]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 4 - 1); int16_t vla[r_1_2]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 5 - 1); int16_t vla[r_1_2]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); int16_t vla[r_1_2]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); int16_t vla[r_2_3]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); int16_t vla[r_2_3]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); int16_t vla[r_2_3]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 4 - 1); int16_t vla[r_2_3]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 5 - 1); int16_t vla[r_2_3]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 6 - 1); int16_t vla[r_2_3]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);



  typedef int int32_t;

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); int32_t vla[r_2_3]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); int32_t vla[r_2_3]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); int32_t vla[r_2_3]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 4 - 1); int32_t vla[r_2_3]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 5 - 1); int32_t vla[r_2_3]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 6 - 1); int32_t vla[r_2_3]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 11 - 1); int32_t vla[r_2_3]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 12 - 1); int32_t vla[r_2_3]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 36 - 1); int32_t vla[r_2_3]; char *d = (char*)vla; strcpy (d, s); sink (vla); } while (0);

}


struct Flex
{
  char n, ax[];
};
# 366 "./Wstringop-overflow-25.c"
__attribute__ ((noipa)) void test_strcpy_malloc_flexarray (void)
{
  size_t r_0_1 = unsigned_range ((0), (1));
  size_t r_1_2 = unsigned_range ((1), (2));
  size_t r_2_3 = unsigned_range ((2), (3));

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); typedef struct { char r_0_1, ax[]; } Flex; Flex *p = (Flex*)malloc (sizeof *p + r_0_1); char *d = (char*)p->ax; strcpy (d, s); sink (p); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); typedef struct { char r_0_1, ax[]; } Flex; Flex *p = (Flex*)malloc (sizeof *p + r_0_1); char *d = (char*)p->ax; strcpy (d, s); sink (p); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); typedef struct { char r_1_2, ax[]; } Flex; Flex *p = (Flex*)malloc (sizeof *p + r_1_2); char *d = (char*)p->ax; strcpy (d, s); sink (p); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); typedef struct { char r_1_2, ax[]; } Flex; Flex *p = (Flex*)malloc (sizeof *p + r_1_2); char *d = (char*)p->ax; strcpy (d, s); sink (p); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); typedef struct { char r_1_2, ax[]; } Flex; Flex *p = (Flex*)malloc (sizeof *p + r_1_2); char *d = (char*)p->ax; strcpy (d, s); sink (p); } while (0);

  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); typedef struct { char r_2_3, ax[]; } Flex; Flex *p = (Flex*)malloc (sizeof *p + r_2_3); char *d = (char*)p->ax; strcpy (d, s); sink (p); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); typedef struct { char r_2_3, ax[]; } Flex; Flex *p = (Flex*)malloc (sizeof *p + r_2_3); char *d = (char*)p->ax; strcpy (d, s); sink (p); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); typedef struct { char r_2_3, ax[]; } Flex; Flex *p = (Flex*)malloc (sizeof *p + r_2_3); char *d = (char*)p->ax; strcpy (d, s); sink (p); } while (0);
  do { char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); typedef struct { char r_2_3, ax[]; } Flex; Flex *p = (Flex*)malloc (sizeof *p + r_2_3); char *d = (char*)p->ax; strcpy (d, s); sink (p); } while (0);
}

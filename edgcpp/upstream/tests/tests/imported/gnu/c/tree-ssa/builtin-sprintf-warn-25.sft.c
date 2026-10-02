//type: fp
//options: 
# 0 "./tree-ssa/builtin-sprintf-warn-25.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/builtin-sprintf-warn-25.c"




# 1 "./tree-ssa/../range.h" 1
# 11 "./tree-ssa/../range.h"
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
# 6 "./tree-ssa/builtin-sprintf-warn-25.c" 2

extern void* alloca (size_t);
extern void* malloc (size_t);

extern int sprintf (char*, const char*, ...);


void sink (void*, ...);

void test_alloca_range (void)
{
  int n1_2 = unsigned_range ((1), (2));
  int n5_9 = unsigned_range ((5), (9));

  char *d = (char*)alloca (n5_9);

  (sprintf (d, "%i", 12345), sink (d));

  d += n1_2;
  (sprintf (d, "%i", 12345), sink (d));

  d += n1_2;
  (sprintf (d, "%i", 12345), sink (d));

  d += n1_2;
  (sprintf (d, "%i", 12345), sink (d));

  d += n1_2;
  (sprintf (d, "%i", 12345), sink (d));

  d += n1_2;
  (sprintf (d, "%i", 12345), sink (d));
}


void test_malloc_range (void)
{
  int n2_3 = unsigned_range ((2), (3));
  int n5_9 = unsigned_range ((5), (9));

  char *d = (char*)malloc (n5_9);

  (sprintf (d, "%i", 12345), sink (d));

  d += n2_3;
  (sprintf (d, "%i", 12345), sink (d));

  d += n2_3;
  (sprintf (d, "%i", 12345), sink (d));

  d += n2_3;
  (sprintf (d, "%i", 12345), sink (d));
}


void test_vla_range (void)
{
  int n3_4 = unsigned_range ((3), (4));
  int n5_9 = unsigned_range ((5), (9));

  char vla[n5_9];
  char *d = vla;

  (sprintf (d, "%i", 12345), sink (d));

  d += n3_4;
  (sprintf (d, "%i", 12345), sink (d));

  d += n3_4;
  (sprintf (d, "%i", 12345), sink (d));
}

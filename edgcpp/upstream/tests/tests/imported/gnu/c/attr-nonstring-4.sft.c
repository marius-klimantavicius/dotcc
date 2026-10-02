//type: fp
//options: 
# 0 "./attr-nonstring-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./attr-nonstring-4.c"
# 11 "./attr-nonstring-4.c"
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
# 12 "./attr-nonstring-4.c" 2

extern size_t strnlen (const char*, size_t);
# 22 "./attr-nonstring-4.c"
void sink (size_t);







void strnlen_cst (void)
{
  size_t n = 0x7fffffffffffffffL;

  do { extern char a34[]; sink (strnlen (a34, n)); } while (0);
  do { extern char a35[]; sink (strnlen (a35, n + 1)); } while (0);

  do { extern char a37[1]; sink (strnlen (a37, n)); } while (0);
  do { extern char a38[2]; sink (strnlen (a38, n + 1)); } while (0);

  do { extern __attribute__ ((nonstring)) char a40[]; sink (strnlen (a40, n)); } while (0);
  do { extern __attribute__ ((nonstring)) char a41[]; sink (strnlen (a41, n + 1)); } while (0);

  do { extern __attribute__ ((nonstring)) char a43[9]; sink (strnlen (a43, n)); } while (0);
  do { extern __attribute__ ((nonstring)) char a44[10]; sink (strnlen (a44, n + 1)); } while (0);
}


void strnlen_range (void)
{
  size_t n = 0x7fffffffffffffffL;
  n = unsigned_range ((n), (n + 1));

  do { extern char a53[]; sink (strnlen (a53, n)); } while (0);
  do { extern char a54[]; sink (strnlen (a54, n + 1)); } while (0);

  do { extern char a56[1]; sink (strnlen (a56, n)); } while (0);
  do { extern char a57[2]; sink (strnlen (a57, n + 1)); } while (0);

  do { extern __attribute__ ((nonstring)) char a59[]; sink (strnlen (a59, n)); } while (0);
  do { extern __attribute__ ((nonstring)) char a60[]; sink (strnlen (a60, n + 1)); } while (0);

  do { extern __attribute__ ((nonstring)) char a62[9]; sink (strnlen (a62, n)); } while (0);
  do { extern __attribute__ ((nonstring)) char a63[10]; sink (strnlen (a63, n + 1)); } while (0);
}

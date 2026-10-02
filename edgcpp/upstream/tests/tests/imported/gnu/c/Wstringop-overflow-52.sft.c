//type: fp
//options: 
# 0 "./Wstringop-overflow-52.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wstringop-overflow-52.c"
# 9 "./Wstringop-overflow-52.c"
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
# 10 "./Wstringop-overflow-52.c" 2

void* memcpy (void*, const void*, size_t);
void* memset (void*, int, size_t);

void sink (void*, ...);

extern char* arrptr[];
extern char* ptr;
extern char* retptr (void);
struct S { char *p; };
extern struct S retstruct (void);

void nowarn_ptr (void)
{
  {
    void *p = arrptr;
    memset (p - 1, 0, 12345);
    memset (p,0, 12345);
    memset (p,0, 0x7fffffffffffffffL - 1);
  }

  {
    char *p = arrptr[0];
    memset (p - 1, 0, 12345);
    memset (p - 12345, 0, 12345);
    memset (p - 1234, 0, 0x7fffffffffffffffL - 1);
    memset (p - 0x7fffffffffffffffL + 1, 0, 12345);
  }

  {
    char *p = ptr;
    memset (p - 1, 0, 12345);
    memset (p - 12345, 0, 12345);
    memset (p - 1234, 0, 0x7fffffffffffffffL - 1);
    memset (p - 0x7fffffffffffffffL + 1, 0, 12345);
  }

  {
    char *p = retptr ();
    memset (p - 1, 0, 12345);
    memset (p - 12345, 0, 12345);
    memset (p - 1234, 0, 0x7fffffffffffffffL - 1);
    memset (p - 0x7fffffffffffffffL + 1, 0, 12345);
  }

  {
    char *p = retstruct ().p;
    memset (p - 1, 0, 12345);
    memset (p - 12345, 0, 12345);
    memset (p - 1234, 0, 0x7fffffffffffffffL - 1);
    memset (p - 0x7fffffffffffffffL + 1, 0, 12345);
  }
}

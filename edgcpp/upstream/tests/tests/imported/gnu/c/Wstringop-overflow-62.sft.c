//type: fp
//options: 
# 0 "./Wstringop-overflow-62.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wstringop-overflow-62.c"




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
# 6 "./Wstringop-overflow-62.c" 2






typedef long unsigned int size_t;

void* memset (void*, int, size_t);


void sink (void*, ...);

volatile int cond, vi;
char* volatile ptr;

void test_min (void)
{
  const int i1 = signed_range ((1), (0x7fffffff));
  const int i2 = signed_range ((2), (0x7fffffff));

  {




    char *p1 = ptr + 1;
    char *p2 = ptr + 2;

    char *q = ((p1) < (p2) ? (p1) : (p2));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 2));
    sink (memset (q, 0, 0x7fffffff));

    sink (memset (q, 0, 0x7fffffffffffffffL - 2));
    sink (memset (q, 0, 0x7fffffffffffffffL));


  }

  {


    char *p1 = ptr + vi;
    char *p2 = ptr + vi;

    char *q = ((p1) < (p2) ? (p1) : (p2));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 2));
    sink (memset (q, 0, 0x7fffffff));
  }

  {


    char a2[2];
    char *p1 = a2 + 1;
    char *p2 = a2 + 2;

    char *q = ((p1) < (p2) ? (p1) : (p2));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 2));
  }

  {


    char a3[3];
    char *pi = a3 + i1;
    char *pj = a3 + i2;

    char *q = ((pi) < (pj) ? (pi) : (pj));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 2));
    sink (memset (q, 0, 3));
  }

  {



    char a4[4];
    char *pi = a4 + vi;
    char *pj = a4 + vi;

    char *q = ((pi) < (pj) ? (pi) : (pj));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 2));
    sink (memset (q, 0, 3));
    sink (memset (q, 0, 4));
    sink (memset (q, 0, 5));
  }

  {


    char a5[5];
    char *p = ptr;
    char *q = ((p) < (a5) ? (p) : (a5));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 2));
    sink (memset (q, 0, 5));
    sink (memset (q, 0, 6));
  }

  {


    char a6[6];
    char *p1 = ptr;
    char *p2 = a6 + 1;
    char *q = ((p1) < (p2) ? (p1) : (p2));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 2));
    sink (memset (q, 0, 6));
    sink (memset (q, 0, 7));
  }

  {


    char a7[7];
    char *p1 = a7;
    char *p2 = ptr + 1;


    char *q = ((p1) < (p2) ? (p1) : (p2));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 2));
    sink (memset (q, 0, 3));
    sink (memset (q, 0, 7));
    sink (memset (q, 0, 8));
  }

  {



    char a8[8];
    char *p1 = a8 + 1;
    char *p2 = ptr + 2;



    char *q = ((p1) < (p2) ? (p1) : (p2));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 2));
    sink (memset (q, 0, 7));
    sink (memset (q, 0, 8));
  }

  {

    char a9[9];
    char *p1 = a9 + 3;
    char *p2 = ptr + 4;


    char *q = ((p1) < (p2) ? (p1) : (p2));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 2));
    sink (memset (q, 0, 6));
    sink (memset (q, 0, 7));
  }

  {

    char a10[10];
    char *p1 = a10 + 10;
    char *p2 = ptr + 5;




    char *q = ((p1) < (p2) ? (p1) : (p2));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 2));
    sink (memset (q, 0, 5));
    sink (memset (q, 0, 6));
  }

  {
    char a3[3];
    char *p1 = ptr;
    char *p2 = a3 + i1;
    char *q = ((p1) < (p2) ? (p1) : (p2));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 2));
    sink (memset (q, 0, 3));
    sink (memset (q, 0, 4));
  }
}


void test_max (void)
{
  const int i1 = signed_range ((1), (0x7fffffff));
  const int i2 = signed_range ((2), (0x7fffffff));

  {


    char a2[2];
    char *pi = a2 + 1;
    char *pj = a2 + 2;

    char *q = ((pi) < (pj) ? (pj) : (pi));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 2));
  }

  {


    char a3[3];
    char *pi = a3 + i1;
    char *pj = a3 + i2;

    char *q = ((pi) < (pj) ? (pj) : (pi));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 2));
    sink (memset (q, 0, 3));
  }

  {



    char a4[4];
    char *pi = a4 + vi;
    char *pj = a4 + vi;

    char *q = ((pi) < (pj) ? (pj) : (pi));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 2));
    sink (memset (q, 0, 3));
    sink (memset (q, 0, 4));
    sink (memset (q, 0, 5));
  }

  {


    char a5[5];
    char *p = ptr;
    char *q = ((p) < (a5) ? (a5) : (p));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 2));
    sink (memset (q, 0, 5));
    sink (memset (q, 0, 6));
  }

  {


    char a6[6];
    char *p1 = ptr;
    char *p2 = a6 + 1;
    char *q = ((p1) < (p2) ? (p2) : (p1));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 5));
    sink (memset (q, 0, 6));
    sink (memset (q, 0, 7));
  }

  {


    char a7[7];
    char *p1 = a7;
    char *p2 = ptr + 1;


    char *q = ((p1) < (p2) ? (p2) : (p1));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 6));
    sink (memset (q, 0, 7));
    sink (memset (q, 0, 8));
  }

  {



    char a8[8];
    char *p1 = a8 + 1;
    char *p2 = ptr + 2;



    char *q = ((p1) < (p2) ? (p2) : (p1));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 6));
    sink (memset (q, 0, 7));
    sink (memset (q, 0, 8));
  }

  {

    char a9[9];
    char *p1 = a9 + 3;
    char *p2 = ptr + 4;


    char *q = ((p1) < (p2) ? (p2) : (p1));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 2));
    sink (memset (q, 0, 5));
    sink (memset (q, 0, 6));
  }

  {

    char a10[10];
    char *p1 = a10 + 10;
    char *p2 = ptr + 5;




    char *q = ((p1) < (p2) ? (p2) : (p1));

    sink (memset (q, 0, 1));
  }

  {
    char a11[11];
    char *p1 = ptr;
    char *p2 = a11 + i1;
    char *q = ((p1) < (p2) ? (p2) : (p1));

    sink (memset (q, 0, 1));
    sink (memset (q, 0, 2));
    sink (memset (q, 0, 10));
    sink (memset (q, 0, 11));
  }
}

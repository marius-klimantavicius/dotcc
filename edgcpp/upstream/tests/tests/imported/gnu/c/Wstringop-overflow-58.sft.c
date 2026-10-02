//type: fp
//options: 
# 0 "./Wstringop-overflow-58.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wstringop-overflow-58.c"





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
# 7 "./Wstringop-overflow-58.c" 2



extern void* memset (void*, int, size_t);


void sink (int, ...);


volatile int cond1, cond2;

extern char ca0[0], ca1[1], ca2[2], ca3[3], ca4[4],
            ca5[5], ca6[6], ca7[7], ca8[8], ca9[9], cax[];







void memset_decl_2 (void)
{
  {
    char *p0_1 = (cond1 ? ca0 : ca1);

    sink (0, memset (p0_1, 0, 0));



    sink (0, memset (p0_1, 0, 1));
    sink (0, memset (p0_1, 0, 2));
    sink (0, memset (p0_1, 0, 9));
  }

  {
    char *p0_x = (cond1 ? ca0 : cax);

    sink (0, memset (p0_x, 0, 0));
    sink (0, memset (p0_x, 0, 1));
    sink (0, memset (p0_x, 0, 2));
    sink (0, memset (p0_x, 0, 9));
  }

  {
    char *p3_5 = (cond1 ? ca3 : ca5);

    sink (0, memset (p3_5, 0, 1));
    sink (0, memset (p3_5, 0, 3));
    sink (0, memset (p3_5, 0, 4));
    sink (0, memset (p3_5, 0, 5));
    sink (0, memset (p3_5, 0, 6));
  }

  {
    char *p5_3 = (cond1 ? ca5 : ca3);

    sink (0, memset (p5_3, 0, 3));
    sink (0, memset (p5_3, 0, 4));
    sink (0, memset (p5_3, 0, 5));
    sink (0, memset (p5_3, 0, 6));
  }

  {
    char *px_3 = (cond1 ? cax : ca3);

    sink (0, memset (px_3, 0, 1));
    sink (0, memset (px_3, 0, 3));
    sink (0, memset (px_3, 0, 4));
    sink (0, memset (px_3, 0, 1234));
  }

  {
    char *p5_x = (cond1 ? ca5 : cax);

    sink (0, memset (p5_x, 0, 1));
    sink (0, memset (p5_x, 0, 5));
    sink (0, memset (p5_x, 0, 6));
    sink (0, memset (p5_x, 0, 1234));
  }

}


void memset_decl_3 (void)
{
  {
    char *p0_1_2 = (cond1 < 0 ? ca0 : 0 < cond1 ? ca1 : ca2);
    sink (0, memset (p0_1_2, 0, 0));
    sink (0, memset (p0_1_2, 0, 1));
    sink (0, memset (p0_1_2, 0, 2));
    sink (0, memset (p0_1_2, 0, 3));
    sink (0, memset (p0_1_2, 0, 9));
  }

  {
    char *p0_2_x = (cond1 < 0 ? ca0 : 0 < cond1 ? ca2 : cax);

    sink (0, memset (p0_2_x, 0, 0));
    sink (0, memset (p0_2_x, 0, 1));
    sink (0, memset (p0_2_x, 0, 3));
    sink (0, memset (p0_2_x, 0, 9));
  }

  {
    char *p3_4_5 = (cond1 < 0 ? ca3 : 0 < cond1 ? ca4 : ca5);

    sink (0, memset (p3_4_5, 0, 3));
    sink (0, memset (p3_4_5, 0, 4));
    sink (0, memset (p3_4_5, 0, 5));
    sink (0, memset (p3_4_5, 0, 6));
  }

  {
    char *p5_3_4 = (cond1 < 0 ? ca5 : 0 < cond1 ? ca3 : ca4);

    sink (0, memset (p5_3_4, 0, 3));
    sink (0, memset (p5_3_4, 0, 4));
    sink (0, memset (p5_3_4, 0, 5));
    sink (0, memset (p5_3_4, 0, 6));
  }

  {
    char *p9_8_7 = (cond1 < 0 ? ca9 : 0 < cond1 ? ca8 : ca7);

    sink (0, memset (p9_8_7, 0, 7));
    sink (0, memset (p9_8_7, 0, 8));
    sink (0, memset (p9_8_7, 0, 9));
    sink (0, memset (p9_8_7, 0, 10));
  }
}





void memset_decl_2_same_size (int i)
{
  {
    char a4_1[4], a4_2[4];
    char *p4 = cond1 ? a4_1 : a4_2;

    sink (0, memset (p4, 0, 1));
    sink (0, memset (p4, 0, 2));
    sink (0, memset (p4, 0, 3));
    sink (0, memset (p4, 0, 4));
    sink (0, memset (p4, 0, 5));
  }

  {
    char a4_1[4];
    char a4_2[4];
    char *p4 = cond1 ? a4_1 : a4_2;
    char *p4_i = p4 + i;

    sink (0, memset (p4_i, 0, 5));
  }

  {
    if (i < 1)
      i = 1;

    char a4_1[4];
    char a4_2[4];
    char *p4 = cond1 ? a4_1 : a4_2;
    char *p4_i = p4 + i;

    sink (0, memset (p4_i, 0, 3));
    sink (0, memset (p4_i, 0, 4));
  }
}


void memset_decl_2_off (void)
{
  int i1 = signed_range ((1), (0x7fffffff));
  int i2 = signed_range ((2), (0x7fffffff));

  {
    char a5[5];
    char a7[7];
    char *p5_p1 = a5 + i1;
    char *p7_p2 = a7 + i2;
    char *p5_7 = cond1 ? p5_p1 : p7_p2;

    sink (0, memset (p5_7, 0, 1));
    sink (0, memset (p5_7, 0, 2));
    sink (0, memset (p5_7, 0, 3));
    sink (0, memset (p5_7, 0, 4));
    sink (0, memset (p5_7, 0, 5));



    sink (0, memset (p5_7, 0, 6));
    sink (0, memset (p5_7, 0, 7));
  }

  int i3 = signed_range ((3), (0x7fffffff));

  {
    char a5[5];




    char a9[9];





    char *p5_p2 = a5 + i2;
    char *p9_p3 = a9 + i3;
    char *p =
      cond1 ? p5_p2 : p9_p3;
    char *q = p + i1;

    sink (0, memset (q, 0, 1));
    sink (0, memset (q, 0, 2));
    sink (0, memset (q, 0, 3));
    sink (0, memset (q, 0, 4));
    sink (0, memset (q, 0, 5));
    sink (0, memset (q, 0, 6));
    sink (0, memset (q, 0, 7));

    --q;
    sink (0, memset (q, 0, 1));
    sink (0, memset (q, 0, 2));
    sink (0, memset (q, 0, 3));
    sink (0, memset (q, 0, 4));
    sink (0, memset (q, 0, 5));
    sink (0, memset (q, 0, 6));
    sink (0, memset (q, 0, 7));
    sink (0, memset (q, 0, 8));

    --q;
    sink (0, memset (q, 0, 1));
    sink (0, memset (q, 0, 2));
    sink (0, memset (q, 0, 3));
    sink (0, memset (q, 0, 4));
    sink (0, memset (q, 0, 5));
    sink (0, memset (q, 0, 6));
    sink (0, memset (q, 0, 7));
    sink (0, memset (q, 0, 8));
    sink (0, memset (q, 0, 9));

    int m1_x = signed_range ((-1), (0x7fffffff));
    int m2_x = signed_range ((-2), (0x7fffffff));

    q += cond2 ? m1_x : m2_x;

    sink (0, memset (q, 0, 1));
    sink (0, memset (q, 0, 2));
    sink (0, memset (q, 0, 3));
    sink (0, memset (q, 0, 4));
    sink (0, memset (q, 0, 5));
    sink (0, memset (q, 0, 6));
    sink (0, memset (q, 0, 7));
    sink (0, memset (q, 0, 8));
    sink (0, memset (q, 0, 9));
    sink (0, memset (q, 0, 10));
  }
}

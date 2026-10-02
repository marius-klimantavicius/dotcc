//type: fp
//options: 
# 0 "./Wstringop-overflow-23.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wstringop-overflow-23.c"
# 9 "./Wstringop-overflow-23.c"
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
# 10 "./Wstringop-overflow-23.c" 2
# 18 "./Wstringop-overflow-23.c"
typedef int int32_t;



__attribute__ ((access (read_only, 2, 1))) void
rd2_1 (int, const void*);

void test_rd2_1 (void)
{
  {
    void *null = 0;
    void *p = &null;

    rd2_1 (0, null);
    rd2_1 (1, p);
  }

  {
    void *null = 0;
    rd2_1 (1, null);
  }

  {
    void *null = 0;




    rd2_1 (signed_range ((1), (2)), null);
  }
}

__attribute__ ((access (write_only, 3, 1))) void
wr3_1 (int, int, void*);

void test_wr3_1 (void)
{
  {
    void *null = 0;
    void *p = &null;

    wr3_1 (signed_range ((0), (1)), 0, null);
    wr3_1 (signed_range ((1), (1)), 0, p);
  }

  void *null = 0;

  wr3_1 (signed_range ((1), (2)), 1, null);
}


__attribute__ ((access (write_only, 2, 1))) void
wr2_1 (int, void*);

void test_wrd2_1 (int n)
{
  wr2_1 (0, 0);
  wr2_1 (signed_range ((-1), (1)), 0);
  wr2_1 (signed_range ((0), (1)), 0);
  wr2_1 (signed_range ((1), (2)), 0);




  wr2_1 (n, 0);
}




struct Incomplete;
extern struct Incomplete inc;

extern char ax[];

__attribute__ ((access (write_only, 1, 2))) void
wr1_2_inc (struct Incomplete*, unsigned);

void test_wr1_2_inc (struct Incomplete *pinc, unsigned n)
{
  wr1_2_inc (0, 0);
  wr1_2_inc (0, 1);

  wr1_2_inc (pinc, 1);
  wr1_2_inc (&inc, 1);

  wr1_2_inc (pinc, 123);
  wr1_2_inc (&inc, 456);

  char a3[3];
  pinc = (struct Incomplete*)a3;
  wr1_2_inc (pinc, signed_range ((3), (4)));
  wr1_2_inc (pinc, signed_range ((4), (5)));


  pinc = (struct Incomplete*)ax;
  wr1_2_inc (pinc, signed_range ((123), (456)));

  char vla[n];
  pinc = (struct Incomplete*)vla;
  wr1_2_inc (pinc, signed_range ((345), (456)));
}


__attribute__ ((access (read_only, 1, 3))) __attribute__ ((access (write_only, 2, 4))) void
rd1_3_wr2_4 (const void*, void*, int, int);

void test_rd1_3_wr2_4 (const void *s, void *d, int n1, int n2)
{
  rd1_3_wr2_4 (s, d, 1, 2);
  rd1_3_wr2_4 (s, d, 123, 456);
  rd1_3_wr2_4 (s, d, 0x7fffffff, 0x7fffffff);
  rd1_3_wr2_4 (s, d, -1, 2);

  const int ir_min_m1 = signed_range (((-0x7fffffff - 1)), (-1));
  rd1_3_wr2_4 (s, d, ir_min_m1, 2);

  rd1_3_wr2_4 (s, d, signed_range ((-1), (0)), 2);
  rd1_3_wr2_4 (s, d, signed_range (((-0x7fffffff - 1)), (0x7fffffff)), 2);

  rd1_3_wr2_4 (s, d, n1, n2);


  const char s11[11] = "0123456789";

  rd1_3_wr2_4 (s11, d, 11, n2);
  rd1_3_wr2_4 (s11, d, 12, n2);

  rd1_3_wr2_4 (s11, d, signed_range ((0), (11)), n2);
  rd1_3_wr2_4 (s11, d, signed_range ((0), (12)), n2);
  rd1_3_wr2_4 (s11, d, signed_range ((11), (12)), n2);
  rd1_3_wr2_4 (s11, d, signed_range ((11), (0x7fffffff)), n2);
  rd1_3_wr2_4 (s11, d, signed_range ((12), (13)), n2);

  char d4[4];
  rd1_3_wr2_4 (s, d4, n1, 4);
  rd1_3_wr2_4 (s, d4, n1, 5);

  rd1_3_wr2_4 (s11, d4, signed_range ((12), (13)), signed_range ((5), (6)));


}




__attribute__ ((access (read_only, 1))) void (*pfrd1)(const void*, const void*);

void test_pfrd1 (void)
{
  pfrd1 ("" + signed_range ((0), (9)), "" + signed_range ((1), (9)));
  pfrd1 ("" + signed_range ((1), (2)), "");
}


__attribute__ ((access (write_only, 4, 3))) void (*pfwr4_3)(int, const char*, int, int32_t*);

void test_pfwr4_3 (void)
{
  int32_t i;
  pfwr4_3 (3, "", 0, &i + signed_range ((0), (9)));
  pfwr4_3 (5, "", 1, &i + signed_range ((1), (2)));
}

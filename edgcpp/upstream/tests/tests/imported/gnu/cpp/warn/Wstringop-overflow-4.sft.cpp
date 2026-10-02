//type: fp
//options: 
# 0 "./warn/Wstringop-overflow-4.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/Wstringop-overflow-4.C"




# 1 "./warn/../../gcc.dg/range.h" 1
# 11 "./warn/../../gcc.dg/range.h"
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
# 6 "./warn/Wstringop-overflow-4.C" 2




extern "C" char* strcpy (char*, const char*);

void sink (void*);
# 25 "./warn/Wstringop-overflow-4.C"
void test_strcpy_new_char (size_t n)
{
  size_t r_0_1 = unsigned_range ((0), (1));
  size_t r_1_2 = unsigned_range ((1), (2));
  size_t r_2_3 = unsigned_range ((2), (3));

  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = (char*)new char[r_0_1]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new char[r_0_1]; strcpy (d, s); sink (d); } while (0);

  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = (char*)new char[r_1_2]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new char[r_1_2]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = (char*)new char[r_1_2]; strcpy (d, s); sink (d); } while (0);

  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = (char*)new char[r_2_3]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = (char*)new char[r_2_3]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = (char*)new char[r_2_3]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = (char*)new char[r_2_3]; strcpy (d, s); sink (d); } while (0);

  size_t r_2_smax = unsigned_range ((2), (0xffffffffffffffffUL));
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = (char*)new char[r_2_smax]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new char[r_2_smax]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = (char*)new char[r_2_smax]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = (char*)new char[r_2_smax * 2]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 4 - 1); char *d = (char*)new char[r_2_smax * 2 + 1]; strcpy (d, s); sink (d); } while (0);

  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new char[n]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = (char*)new char[n + 1]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = (char*)new char[n * 2 + 1]; strcpy (d, s); sink (d); } while (0);

  int r_imin_imax = signed_range (((-0x7fffffff - 1)), (0x7fffffff));
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new char[r_imin_imax]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = (char*)new char[r_imin_imax + 1]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = (char*)new char[r_imin_imax * 2 + 1]; strcpy (d, s); sink (d); } while (0);

  int r_0_imax = signed_range ((0), (0x7fffffff));
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new char[r_0_imax]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = (char*)new char[r_0_imax + 1]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = (char*)new char[r_0_imax * 2 + 1]; strcpy (d, s); sink (d); } while (0);

  int r_1_imax = signed_range ((1), (0x7fffffff));
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new char[r_1_imax]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = (char*)new char[r_1_imax + 1]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = (char*)new char[r_1_imax * 2 + 1]; strcpy (d, s); sink (d); } while (0);

  ptrdiff_t r_dmin_dmax = signed_range (((-0x7fffffffffffffffL - 1)), (0x7fffffffffffffffL));
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new char[r_dmin_dmax]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = (char*)new char[r_dmin_dmax + 1]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = (char*)new char[r_dmin_dmax * 2 + 1]; strcpy (d, s); sink (d); } while (0);
}


void test_strcpy_new_char_array (size_t n)
{
  size_t r_0_1 = unsigned_range ((0), (1));

  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = (char*)new char[r_0_1][1]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new char[r_0_1][1]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new char[r_0_1][2]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = (char*)new char[r_0_1][2]; strcpy (d, s); sink (d); } while (0);

  size_t r_1_2 = unsigned_range ((1), (2));
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = (char*)new char[r_1_2][0]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = (char*)new char[r_1_2][1]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new char[r_1_2][1]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = (char*)new char[r_1_2][1]; strcpy (d, s); sink (d); } while (0);

  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = (char*)new char[r_1_2][0]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = (char*)new char[r_1_2][1]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new char[r_1_2][2]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = (char*)new char[r_1_2][2]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 4 - 1); char *d = (char*)new char[r_1_2][2]; strcpy (d, s); sink (d); } while (0);
}
# 105 "./warn/Wstringop-overflow-4.C"
typedef short int int16_t;

void test_strcpy_new_int16_t (size_t n, const size_t vals[])
{
  size_t idx = 0;

  size_t r_0_1 = (++idx, (vals[idx] < 0 || 1 < vals[idx] ? 0 : vals[idx]));
  size_t r_1_2 = (++idx, (vals[idx] < 1 || 2 < vals[idx] ? 1 : vals[idx]));
  size_t r_2_3 = (++idx, (vals[idx] < 2 || 3 < vals[idx] ? 2 : vals[idx]));

  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = (char*)new int16_t[r_0_1]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new int16_t[r_0_1]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = (char*)new int16_t[r_0_1]; strcpy (d, s); sink (d); } while (0);

  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = (char*)new int16_t[r_1_2]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new int16_t[r_1_2]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = (char*)new int16_t[r_1_2]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = (char*)new int16_t[r_1_2]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 4 - 1); char *d = (char*)new int16_t[r_1_2]; strcpy (d, s); sink (d); } while (0);

  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = (char*)new int16_t[r_2_3]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new int16_t[r_2_3]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 5 - 1); char *d = (char*)new int16_t[r_2_3]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 6 - 1); char *d = (char*)new int16_t[r_2_3]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = (char*)new int16_t[r_2_3]; strcpy (d, s); sink (d); } while (0);

  size_t r_2_smax = (++idx, (vals[idx] < 2 || 0xffffffffffffffffUL < vals[idx] ? 2 : vals[idx]));
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 0 - 1); char *d = (char*)new int16_t[r_2_smax]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new int16_t[r_2_smax]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = (char*)new int16_t[r_2_smax]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 3 - 1); char *d = (char*)new int16_t[r_2_smax * 2]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 4 - 1); char *d = (char*)new int16_t[r_2_smax * 2 + 1]; strcpy (d, s); sink (d); } while (0);

  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new int16_t[n]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = (char*)new int16_t[n + 1]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = (char*)new int16_t[n * 2 + 1]; strcpy (d, s); sink (d); } while (0);

  int r_imin_imax = signed_range (((-0x7fffffff - 1)), (0x7fffffff));
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new int16_t[r_imin_imax]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = (char*)new int16_t[r_imin_imax + 1]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = (char*)new int16_t[r_imin_imax * 2 + 1]; strcpy (d, s); sink (d); } while (0);

  int r_0_imax = signed_range ((0), (0x7fffffff));

  if (sizeof (int) < sizeof (size_t))


    do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new int16_t[r_0_imax]; strcpy (d, s); sink (d); } while (0);



  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = (char*)new int16_t[r_0_imax + 1]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = (char*)new int16_t[r_0_imax * 2 + 1]; strcpy (d, s); sink (d); } while (0);

  int r_1_imax = signed_range ((1), (0x7fffffff));
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new int16_t[r_1_imax]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = (char*)new int16_t[r_1_imax + 1]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = (char*)new int16_t[r_1_imax * 2 + 1]; strcpy (d, s); sink (d); } while (0);

  ptrdiff_t r_dmin_dmax = signed_range (((-0x7fffffffffffffffL - 1)), (0x7fffffffffffffffL));
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 1 - 1); char *d = (char*)new int16_t[r_dmin_dmax]; strcpy (d, s); sink (d); } while (0);
# 200 "./warn/Wstringop-overflow-4.C"
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 2 - 1); char *d = (char*)new int16_t[r_dmin_dmax + 1]; strcpy (d, s); sink (d); } while (0);
  do { const char *s = ("0123456789abcdefghijklmnopqrstuvwxyz" + sizeof "0123456789abcdefghijklmnopqrstuvwxyz" - 9 - 1); char *d = (char*)new int16_t[r_dmin_dmax * 2 + 1]; strcpy (d, s); sink (d); } while (0);
}

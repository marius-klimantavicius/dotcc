//type: fp
//options: 
# 0 "./attr-nonstring-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./attr-nonstring-2.c"




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
# 6 "./attr-nonstring-2.c" 2

extern void* memcpy (void*, const void*, size_t);
extern size_t strnlen (const char*, size_t);







void sink (size_t, ...);



void test_strnlen_array_cst (void)
{
  __attribute__ ((nonstring)) char ns3[3];
  sink (0, ns3);

  sink (strnlen (ns3, 0));
  sink (strnlen (ns3, 1));
  sink (strnlen (ns3, 2));
  sink (strnlen (ns3, 3));
  sink (strnlen (ns3, 4));
  sink (strnlen (ns3, 0x7fffffffffffffffL));
  sink (strnlen (ns3, 0xffffffffffffffffUL));

  __attribute__ ((nonstring)) char ns5[5];
  sink (0, ns5);

  sink (strnlen (ns5, 0));
  sink (strnlen (ns5, 1));
  sink (strnlen (ns5, 2));
  sink (strnlen (ns5, 3));
  sink (strnlen (ns5, 6));
  sink (strnlen (ns5, 0x7fffffffffffffffL));
  sink (strnlen (ns5, 0xffffffffffffffffUL));
}


void test_strnlen_array_range (void)
{
  __attribute__ ((nonstring)) char ns3[3];
  sink (0, ns3);

  sink (strnlen (ns3, unsigned_range ((0), (3))));
  sink (strnlen (ns3, unsigned_range ((0), (9))));
  sink (strnlen (ns3, unsigned_range ((3), (4))));
  sink (strnlen (ns3, unsigned_range ((3), (0x7fffffffffffffffL))));
  sink (strnlen (ns3, unsigned_range ((4), (5))));
  sink (strnlen (ns3, unsigned_range ((0x7fffffffffffffffL), (0xffffffffffffffffUL))));
}
# 68 "./attr-nonstring-2.c"
void test_strnlen_string_cst (void)
{
  do { extern __attribute__ ((nonstring)) char arr70[3]; memcpy (arr70, "1", 2); sink (strnlen (arr70, 1), arr70); } while (0);
  do { extern __attribute__ ((nonstring)) char arr71[3]; memcpy (arr71, "1", 2); sink (strnlen (arr71, 2), arr71); } while (0);
  do { extern __attribute__ ((nonstring)) char arr72[3]; memcpy (arr72, "1", 2); sink (strnlen (arr72, 3), arr72); } while (0);
  do { extern __attribute__ ((nonstring)) char arr73[3]; memcpy (arr73, "12", 3); sink (strnlen (arr73, 1), arr73); } while (0);
  do { extern __attribute__ ((nonstring)) char arr74[3]; memcpy (arr74, "12", 3); sink (strnlen (arr74, 9), arr74); } while (0);
  do { extern __attribute__ ((nonstring)) char arr75[3]; memcpy (arr75, "123", 3); sink (strnlen (arr75, 1), arr75); } while (0);
  do { extern __attribute__ ((nonstring)) char arr76[3]; memcpy (arr76, "123", 3); sink (strnlen (arr76, 4), arr76); } while (0);
  do { extern __attribute__ ((nonstring)) char arr77[3]; memcpy (arr77, "123", 3); sink (strnlen (arr77, 9), arr77); } while (0);

  do { extern __attribute__ ((nonstring)) char arr79[5]; memcpy (arr79, "1", 2); sink (strnlen (arr79, 1), arr79); } while (0);
  do { extern __attribute__ ((nonstring)) char arr80[5]; memcpy (arr80, "1", 2); sink (strnlen (arr80, 2), arr80); } while (0);
  do { extern __attribute__ ((nonstring)) char arr81[5]; memcpy (arr81, "1", 2); sink (strnlen (arr81, 9), arr81); } while (0);

  do { extern __attribute__ ((nonstring)) char arr83[5]; memcpy (arr83, "12", 3); sink (strnlen (arr83, 1), arr83); } while (0);
  do { extern __attribute__ ((nonstring)) char arr84[5]; memcpy (arr84, "12", 3); sink (strnlen (arr84, 9), arr84); } while (0);
  do { extern __attribute__ ((nonstring)) char arr85[5]; memcpy (arr85, "123", 3); sink (strnlen (arr85, 1), arr85); } while (0);
  do { extern __attribute__ ((nonstring)) char arr86[5]; memcpy (arr86, "123", 3); sink (strnlen (arr86, 5), arr86); } while (0);
  do { extern __attribute__ ((nonstring)) char arr87[5]; memcpy (arr87, "123", 3); sink (strnlen (arr87, 6), arr87); } while (0);




  do { extern __attribute__ ((nonstring)) char arr92[]; memcpy (arr92, "1", 1); sink (strnlen (arr92, 1), arr92); } while (0);
  do { extern __attribute__ ((nonstring)) char arr93[]; memcpy (arr93, "1", 1); sink (strnlen (arr93, 2), arr93); } while (0);
  do { extern __attribute__ ((nonstring)) char arr94[]; memcpy (arr94, "1", 2); sink (strnlen (arr94, 1), arr94); } while (0);
  do { extern __attribute__ ((nonstring)) char arr95[]; memcpy (arr95, "1", 2); sink (strnlen (arr95, 2), arr95); } while (0);
  do { extern __attribute__ ((nonstring)) char arr96[]; memcpy (arr96, "1", 2); sink (strnlen (arr96, 3), arr96); } while (0);
  do { extern __attribute__ ((nonstring)) char arr97[]; memcpy (arr97, "1", 2); sink (strnlen (arr97, 9), arr97); } while (0);
  do { extern __attribute__ ((nonstring)) char arr98[]; memcpy (arr98, "1", 2); sink (strnlen (arr98, 0x7fffffffffffffffL), arr98); } while (0);
  do { extern __attribute__ ((nonstring)) char arr99[]; memcpy (arr99, "1", 2); sink (strnlen (arr99, 0xffffffffffffffffUL), arr99); } while (0);

  size_t n = 0x7fffffffffffffffL;
  do { extern __attribute__ ((nonstring)) char arr102[]; memcpy (arr102, "123", 3); sink (strnlen (arr102, n), arr102); } while (0);
  do { extern __attribute__ ((nonstring)) char arr103[]; memcpy (arr103, "123", 3); sink (strnlen (arr103, n + 1), arr103); } while (0);
  n = 0xffffffffffffffffUL;
  do { extern __attribute__ ((nonstring)) char arr105[]; memcpy (arr105, "123", 3); sink (strnlen (arr105, n), arr105); } while (0);
}


void test_strnlen_string_range (void)
{
  do { extern __attribute__ ((nonstring)) char arr111[3]; memcpy (arr111, "1", 2); sink (strnlen (arr111, unsigned_range ((0), (1))), arr111); } while (0);
  do { extern __attribute__ ((nonstring)) char arr112[3]; memcpy (arr112, "1", 2); sink (strnlen (arr112, unsigned_range ((3), (9))), arr112); } while (0);
  do { extern __attribute__ ((nonstring)) char arr113[3]; memcpy (arr113, "123", 3); sink (strnlen (arr113, unsigned_range ((4), (5))), arr113); } while (0);
  do { extern __attribute__ ((nonstring)) char arr114[3]; memcpy (arr114, "123", 3); sink (strnlen (arr114, unsigned_range ((5), (9))), arr114); } while (0);
}

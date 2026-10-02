//type: fp
//options: 
# 0 "./attr-nonstring-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./attr-nonstring-3.c"
# 13 "./attr-nonstring-3.c"
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
# 14 "./attr-nonstring-3.c" 2

extern int strncmp (const char*, const char*, size_t);
# 24 "./attr-nonstring-3.c"
void sink (int);
# 33 "./attr-nonstring-3.c"
void strncmp_cst (void)
{
  size_t n = 0x7fffffffffffffffL;

  do { extern char a37[]; extern char b37[]; sink (strncmp (a37, b37, n)); } while (0);
  do { extern char a38[]; extern char b38[]; sink (strncmp (a38, b38, n + 1)); } while (0);

  do { extern char a40[1]; extern char b40[]; sink (strncmp (a40, b40, 1)); } while (0);
  do { extern char a41[1]; extern char b41[]; sink (strncmp (a41, b41, n)); } while (0);
  do { extern char a42[2]; extern char b42[]; sink (strncmp (a42, b42, n + 1)); } while (0);

  do { extern char a44[]; extern char b44[3]; sink (strncmp (a44, b44, 3)); } while (0);
  do { extern char a45[]; extern char b45[3]; sink (strncmp (a45, b45, n)); } while (0);
  do { extern char a46[]; extern char b46[4]; sink (strncmp (a46, b46, n + 1)); } while (0);

  do { extern char a48[]; extern __attribute__ ((nonstring)) char b48[]; sink (strncmp (a48, b48, 3)); } while (0);
  do { extern char a49[]; extern __attribute__ ((nonstring)) char b49[]; sink (strncmp (a49, b49, n)); } while (0);
  do { extern char a50[]; extern __attribute__ ((nonstring)) char b50[]; sink (strncmp (a50, b50, n + 1)); } while (0);

  do { extern char a52[5]; extern __attribute__ ((nonstring)) char b52[]; sink (strncmp (a52, b52, 4)); } while (0);
  do { extern char a53[5]; extern __attribute__ ((nonstring)) char b53[]; sink (strncmp (a53, b53, 5)); } while (0);
  do { extern char a54[5]; extern __attribute__ ((nonstring)) char b54[]; sink (strncmp (a54, b54, 6)); } while (0);
  do { extern char a55[5]; extern __attribute__ ((nonstring)) char b55[]; sink (strncmp (a55, b55, n)); } while (0);
  do { extern char a56[6]; extern __attribute__ ((nonstring)) char b56[]; sink (strncmp (a56, b56, n + 1)); } while (0);

  do { extern char a58[]; extern __attribute__ ((nonstring)) char b58[7]; sink (strncmp (a58, b58, n)); } while (0);

  do { extern char a60[]; extern __attribute__ ((nonstring)) char b60[8]; sink (strncmp (a60, b60, n + 1)); } while (0);

  do { extern __attribute__ ((nonstring)) char a62[]; extern char b62[]; sink (strncmp (a62, b62, n)); } while (0);
  do { extern __attribute__ ((nonstring)) char a63[]; extern char b63[]; sink (strncmp (a63, b63, n + 1)); } while (0);

  do { extern __attribute__ ((nonstring)) char a65[9]; extern char b65[]; sink (strncmp (a65, b65, n)); } while (0);
  do { extern __attribute__ ((nonstring)) char a66[10]; extern char b66[]; sink (strncmp (a66, b66, n + 1)); } while (0);

  do { extern __attribute__ ((nonstring)) char a68[]; extern char b68[11]; sink (strncmp (a68, b68, 11)); } while (0);
  do { extern __attribute__ ((nonstring)) char a69[]; extern char b69[11]; sink (strncmp (a69, b69, n)); } while (0);
  do { extern __attribute__ ((nonstring)) char a70[]; extern char b70[12]; sink (strncmp (a70, b70, n + 1)); } while (0);

  do { extern __attribute__ ((nonstring)) char a72[]; extern __attribute__ ((nonstring)) char b72[]; sink (strncmp (a72, b72, n)); } while (0);
  do { extern __attribute__ ((nonstring)) char a73[]; extern __attribute__ ((nonstring)) char b73[]; sink (strncmp (a73, b73, n + 1)); } while (0);

  do { extern __attribute__ ((nonstring)) char a75[13]; extern __attribute__ ((nonstring)) char b75[]; sink (strncmp (a75, b75, 13)); } while (0);
  do { extern __attribute__ ((nonstring)) char a76[13]; extern __attribute__ ((nonstring)) char b76[]; sink (strncmp (a76, b76, n)); } while (0);
  do { extern __attribute__ ((nonstring)) char a77[14]; extern __attribute__ ((nonstring)) char b77[]; sink (strncmp (a77, b77, n + 1)); } while (0);

  do { extern __attribute__ ((nonstring)) char a79[]; extern __attribute__ ((nonstring)) char b79[15]; sink (strncmp (a79, b79, 15)); } while (0);
  do { extern __attribute__ ((nonstring)) char a80[]; extern __attribute__ ((nonstring)) char b80[15]; sink (strncmp (a80, b80, 16)); } while (0);
  do { extern __attribute__ ((nonstring)) char a81[]; extern __attribute__ ((nonstring)) char b81[16]; sink (strncmp (a81, b81, n + 1)); } while (0);
}


void strncmp_range (void)
{
  size_t n = 0x7fffffffffffffffL;
  n = unsigned_range ((n), (n + 1));

  do { extern char a90[]; extern char b90[]; sink (strncmp (a90, b90, n)); } while (0);
  do { extern char a91[]; extern char b91[]; sink (strncmp (a91, b91, n + 1)); } while (0);

  do { extern char a93[1]; extern char b93[]; sink (strncmp (a93, b93, 1)); } while (0);
  do { extern char a94[1]; extern char b94[]; sink (strncmp (a94, b94, n)); } while (0);
  do { extern char a95[2]; extern char b95[]; sink (strncmp (a95, b95, n + 1)); } while (0);

  do { extern char a97[]; extern char b97[3]; sink (strncmp (a97, b97, n)); } while (0);
  do { extern char a98[]; extern char b98[4]; sink (strncmp (a98, b98, n + 1)); } while (0);

  do { extern char a100[]; extern __attribute__ ((nonstring)) char b100[]; sink (strncmp (a100, b100, n)); } while (0);
  do { extern char a101[]; extern __attribute__ ((nonstring)) char b101[]; sink (strncmp (a101, b101, n + 1)); } while (0);

  do { extern char a103[5]; extern __attribute__ ((nonstring)) char b103[]; sink (strncmp (a103, b103, n)); } while (0);
  do { extern char a104[6]; extern __attribute__ ((nonstring)) char b104[]; sink (strncmp (a104, b104, n + 1)); } while (0);

  do { extern char a106[]; extern __attribute__ ((nonstring)) char b106[7]; sink (strncmp (a106, b106, n)); } while (0);

  do { extern char a108[]; extern __attribute__ ((nonstring)) char b108[8]; sink (strncmp (a108, b108, n + 1)); } while (0);

  do { extern __attribute__ ((nonstring)) char a110[]; extern char b110[]; sink (strncmp (a110, b110, n)); } while (0);
  do { extern __attribute__ ((nonstring)) char a111[]; extern char b111[]; sink (strncmp (a111, b111, n + 1)); } while (0);

  do { extern __attribute__ ((nonstring)) char a113[9]; extern char b113[]; sink (strncmp (a113, b113, n)); } while (0);
  do { extern __attribute__ ((nonstring)) char a114[10]; extern char b114[]; sink (strncmp (a114, b114, n + 1)); } while (0);

  do { extern __attribute__ ((nonstring)) char a116[]; extern char b116[11]; sink (strncmp (a116, b116, n)); } while (0);
  do { extern __attribute__ ((nonstring)) char a117[]; extern char b117[12]; sink (strncmp (a117, b117, n + 1)); } while (0);

  do { extern __attribute__ ((nonstring)) char a119[]; extern __attribute__ ((nonstring)) char b119[]; sink (strncmp (a119, b119, n)); } while (0);
  do { extern __attribute__ ((nonstring)) char a120[]; extern __attribute__ ((nonstring)) char b120[]; sink (strncmp (a120, b120, n + 1)); } while (0);

  do { extern __attribute__ ((nonstring)) char a122[13]; extern __attribute__ ((nonstring)) char b122[]; sink (strncmp (a122, b122, n)); } while (0);
  do { extern __attribute__ ((nonstring)) char a123[14]; extern __attribute__ ((nonstring)) char b123[]; sink (strncmp (a123, b123, n + 1)); } while (0);

  do { extern __attribute__ ((nonstring)) char a125[]; extern __attribute__ ((nonstring)) char b125[15]; sink (strncmp (a125, b125, n)); } while (0);
  do { extern __attribute__ ((nonstring)) char a126[]; extern __attribute__ ((nonstring)) char b126[16]; sink (strncmp (a126, b126, n + 1)); } while (0);
}

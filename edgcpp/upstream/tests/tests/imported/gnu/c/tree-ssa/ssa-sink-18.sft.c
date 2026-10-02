//type: fp
//options: 
# 0 "./tree-ssa/ssa-sink-18.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/ssa-sink-18.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 1 3 4
# 9 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 3 4
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/usr/include/stdint.h" 1 3 4
# 25 "/usr/include/stdint.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
# 375 "/usr/include/features.h" 3 4
# 1 "/usr/include/sys/cdefs.h" 1 3 4
# 392 "/usr/include/sys/cdefs.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 393 "/usr/include/sys/cdefs.h" 2 3 4
# 376 "/usr/include/features.h" 2 3 4
# 399 "/usr/include/features.h" 3 4
# 1 "/usr/include/gnu/stubs.h" 1 3 4
# 10 "/usr/include/gnu/stubs.h" 3 4
# 1 "/usr/include/gnu/stubs-64.h" 1 3 4
# 11 "/usr/include/gnu/stubs.h" 2 3 4
# 400 "/usr/include/features.h" 2 3 4
# 26 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wchar.h" 1 3 4
# 22 "/usr/include/bits/wchar.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 23 "/usr/include/bits/wchar.h" 2 3 4
# 27 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 28 "/usr/include/stdint.h" 2 3 4
# 36 "/usr/include/stdint.h" 3 4
typedef signed char int8_t;
typedef short int int16_t;
typedef int int32_t;

typedef long int int64_t;







typedef unsigned char uint8_t;
typedef unsigned short int uint16_t;

typedef unsigned int uint32_t;



typedef unsigned long int uint64_t;
# 65 "/usr/include/stdint.h" 3 4
typedef signed char int_least8_t;
typedef short int int_least16_t;
typedef int int_least32_t;

typedef long int int_least64_t;






typedef unsigned char uint_least8_t;
typedef unsigned short int uint_least16_t;
typedef unsigned int uint_least32_t;

typedef unsigned long int uint_least64_t;
# 90 "/usr/include/stdint.h" 3 4
typedef signed char int_fast8_t;

typedef long int int_fast16_t;
typedef long int int_fast32_t;
typedef long int int_fast64_t;
# 103 "/usr/include/stdint.h" 3 4
typedef unsigned char uint_fast8_t;

typedef unsigned long int uint_fast16_t;
typedef unsigned long int uint_fast32_t;
typedef unsigned long int uint_fast64_t;
# 119 "/usr/include/stdint.h" 3 4
typedef long int intptr_t;


typedef unsigned long int uintptr_t;
# 134 "/usr/include/stdint.h" 3 4
typedef long int intmax_t;
typedef unsigned long int uintmax_t;
# 12 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 2 3 4
#pragma GCC diagnostic pop
# 6 "./tree-ssa/ssa-sink-18.c" 2




# 9 "./tree-ssa/ssa-sink-18.c"
typedef const uint8_t *LZF_HSLOT;
typedef LZF_HSLOT LZF_STATE[1 << (16)];

int
compute_on_bytes (uint8_t *in_data, int in_len, uint8_t *out_data, int out_len)
{
  LZF_STATE htab;

  uint8_t *ip = in_data;
  uint8_t *op = out_data;
  uint8_t *in_end = ip + in_len;
  uint8_t *out_end = op + out_len;
  uint8_t *ref;

  unsigned long off;
  unsigned int hval;
  int lit;

  if (!in_len || !out_len)
    return 0;

  lit = 0;
  op++;
  hval = (((ip[0]) << 8) | ip[1]);

  while (ip < in_end - 2)
    {
      uint8_t *hslot;

      hval = (((hval) << 8) | ip[2]);
      hslot = (uint8_t*)(htab + (((hval >> (3 * 8 - 16)) - hval * 5) & ((1 << (16)) - 1)));

      ref = *hslot + in_data;
      *hslot = ip - in_data;

      if (1 && (off = ip - ref - 1) < (1 << 13) && ref > in_data
   && ref[2] == ip[2]
   && ((ref[1] << 8) | ref[0]) == ((ip[1] << 8) | ip[0]))
 {
   unsigned int len = 2;
   unsigned int maxlen = in_end - ip - len;
   maxlen
     = maxlen > ((1 << 8) + (1 << 3)) ? ((1 << 8) + (1 << 3)) : maxlen;

   if ((op + 3 + 1 >= out_end) != 0)
     if (op - !lit + 3 + 1 >= out_end)
       return 0;

   op[-lit - 1] = lit - 1;
   op -= !lit;

   for (;;)
     {
       if (maxlen > 16)
  {
    len++;
    if (ref[len] != ip[len])
      break;
    len++;
    if (ref[len] != ip[len])
      break;
    len++;
    if (ref[len] != ip[len])
      break;
    len++;
    if (ref[len] != ip[len])
      break;

    len++;
    if (ref[len] != ip[len])
      break;
    len++;
    if (ref[len] != ip[len])
      break;
    len++;
    if (ref[len] != ip[len])
      break;
    len++;
    if (ref[len] != ip[len])
      break;

    len++;
    if (ref[len] != ip[len])
      break;
    len++;
    if (ref[len] != ip[len])
      break;
    len++;
    if (ref[len] != ip[len])
      break;
    len++;
    if (ref[len] != ip[len])
      break;

    len++;
    if (ref[len] != ip[len])
      break;
    len++;
    if (ref[len] != ip[len])
      break;
    len++;
    if (ref[len] != ip[len])
      break;
    len++;
    if (ref[len] != ip[len])
      break;
  }

       do
  {
    len++;
  }
       while (len < maxlen && ip[len] == ref[len]);

       break;
     }

   len -= 2;
   ip++;

   if (len < 7)
     {
       *op++ = (off >> 8) + (len << 5);
     }
   else
     {
       *op++ = (off >> 8) + (7 << 5);
       *op++ = len - 7;
     }
   *op++ = off;
   lit = 0;
   op++;
   ip += len + 1;

   if (ip >= in_end - 2)
     break;

   --ip;
   --ip;

   hval = (((ip[0]) << 8) | ip[1]);
   hval = (((hval) << 8) | ip[2]);
   htab[(((hval >> (3 * 8 - 16)) - hval * 5) & ((1 << (16)) - 1))]
     = (LZF_HSLOT)(ip - in_data);
   ip++;

   hval = (((hval) << 8) | ip[2]);
   htab[(((hval >> (3 * 8 - 16)) - hval * 5) & ((1 << (16)) - 1))]
     = (LZF_HSLOT)(ip - in_data);
   ip++;
 }
      else
 {
   if (op >= out_end)
     return 0;

   lit++;
   *op++ = *ip++;

   if (lit == (1 << 5))
     {
       op[-lit - 1] = lit - 1;
       lit = 0;
       op++;
     }
 }
    }
  if (op + 3 > out_end)
    return 0;

  while (ip < in_end)
    {
      lit++;
      *op++ = *ip++;
      if (lit == (1 << 5))
 {
   op[-lit - 1] = lit - 1;
   lit = 0;
   op++;
 }
    }

  op[-lit - 1] = lit - 1;
  op -= !lit;

  return op - out_data;
 }

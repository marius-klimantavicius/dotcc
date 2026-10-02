//type: rp
//options: 
# 0 "./torture/crc-crc8-data8-loop-xorOutsideFor.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/crc-crc8-data8-loop-xorOutsideFor.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef int wchar_t;
# 425 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
} max_align_t;
# 450 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
  typedef __typeof__(nullptr) nullptr_t;
# 6 "./torture/crc-crc8-data8-loop-xorOutsideFor.c" 2


# 7 "./torture/crc-crc8-data8-loop-xorOutsideFor.c"
typedef unsigned char uint8_t;

uint8_t gencrc (uint8_t *message, size_t len) {
  uint8_t crc = 0;
  size_t i, j;
  for (i = 0; i < len; i++) {
      uint8_t data = message[i];
      crc ^= data;
      for (j = 0; j < 8; j++) {
   if ((crc & 0x80)!= 0)
     crc = (uint8_t) ((crc << 1) ^ 0x31);
   else
     crc <<= 1;
 }
    }
  return crc;
}

int main()
{
  uint8_t message[] = "Hello world!";
  if (gencrc(message, 12) != 0x24)
    __builtin_abort ();
  __builtin_exit (0);
}

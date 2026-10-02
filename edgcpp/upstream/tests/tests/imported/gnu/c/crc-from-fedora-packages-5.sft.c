//type: fp
//options: 
# 0 "./crc-from-fedora-packages-5.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./crc-from-fedora-packages-5.c"






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
# 8 "./crc-from-fedora-packages-5.c" 2

# 8 "./crc-from-fedora-packages-5.c"
typedef unsigned int u32;
u32 gf2_multiply(u32 x, u32 y, u32 modulus)
{
    u32 product = x & 1 ? y : 0;
    int i;

    for (i = 0; i < 31; i++) {
        product = (product >> 1) ^ (product & 1 ? modulus : 0);
        x >>= 1;
        product ^= x & 1 ? y : 0;
    }

    return product;
}

u32 crc32_generic_shift(u32 crc, size_t len,
                        u32 polynomial)
{
    u32 power = 0x2101;
    int i;

    for (i = 0; i < 8 * (int)(len & 3); i++)
        crc = (crc >> 1) ^ (crc & 1 ? 0x2101 : 0);

    len >>= 2;
    if (!len)
        return crc;

    for (;;) {

        if (len & 1)
            crc = gf2_multiply(crc, power, polynomial);

        len >>= 1;
        if (!len)
            break;


        power = gf2_multiply(power, power, polynomial);
    }

    return crc;
}

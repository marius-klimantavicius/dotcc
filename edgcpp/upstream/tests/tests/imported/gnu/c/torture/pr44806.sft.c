//type: rp
//options: --c99
# 0 "./torture/pr44806.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/pr44806.c"



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
# 5 "./torture/pr44806.c" 2


# 6 "./torture/pr44806.c"
extern void abort (void);







struct dev_ino_4
{
  uint32_t mode:2;
  uint32_t short_ino:(32 - 5 - 2 - 1);
  uint32_t mapped_dev:5;
  uint32_t always_set:1;
};

struct dev_ino_8
{
  uint32_t mode:2;
  uint64_t short_ino:(64 - 8 - 2 - 1);
  uint32_t mapped_dev:8;
  uint32_t always_set:1;
};

struct dev_ino_full
{
  uint32_t mode:2;
  uint32_t dev;
  uint32_t ino;
};

enum di_mode
{
  DI_MODE_4 = 1,
  DI_MODE_8 = 2,
  DI_MODE_FULL = 3
};

struct di_ent
{
  union
  {
    struct dev_ino_4 di4;
    struct dev_ino_8 di8;
    struct dev_ino_full full;
    uint32_t u32;
    uint64_t u64;
    void *ptr;
  } u;
};

static struct di_ent
decode_ptr (struct di_ent const *v)
{
  struct di_ent di;
  di.u.ptr = (void *) v;
  return di;
}

static int
di_ent_equal (void const *x, void const *y)
{
  struct di_ent a = decode_ptr (x);
  struct di_ent b = decode_ptr (y);
  if (a.u.di4.mode != b.u.di4.mode)
    return 0;

  if (a.u.di4.mode == DI_MODE_4)
    return (a.u.di4.short_ino == b.u.di4.short_ino
            && a.u.di4.mapped_dev == b.u.di4.mapped_dev);

  if (a.u.di8.mode == DI_MODE_8)
    return (a.u.di8.short_ino == b.u.di8.short_ino
            && a.u.di8.mapped_dev == b.u.di8.mapped_dev);

  return (a.u.full.ino == b.u.full.ino
          && a.u.full.dev == b.u.full.dev);
}

int
main ()
{
  if (di_ent_equal ((void *) 0x80143c4d, (void *) 0x80173851) != 0)
    abort ();
  return 0;
}

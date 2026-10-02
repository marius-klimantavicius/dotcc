//type: rp
//options: -O
# 0 "./pr27959-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr27959-2.c"





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
# 7 "./pr27959-2.c" 2

# 7 "./pr27959-2.c"
extern void abort (void);

struct B
{
  unsigned int b1, b2, b3;
  char b4;
};

struct C
{
  char c1;
};

struct D
{
  char *d1;
  struct C **d2;
  unsigned int d3;
};

void
__attribute__((noinline))
foo (void *x, struct B *y, unsigned int *z)
{
  if (x)
    abort ();
  if (y->b1 != 7 || y->b2 != 5 || y->b3 != 3 || y->b4)
    abort ();
  if (*z != 2)
    abort ();
}

int
__attribute__((noinline))
baz (unsigned int *x, unsigned int y)
{
  asm volatile ("" : : "r" (&x), "r" (&y) : "memory");
  return *x + y;
}

inline int bar (unsigned int *x, unsigned int y)
{
  if (y < *x)
    return 0;
  return baz (x, y);
}

unsigned int *
__attribute__((noinline))
test (struct D *x, unsigned int *y)
{
  struct B b;
  uint32_t c;

  bar (y, x->d3);
  if ((*(x->d2))->c1)
    c = ((uint32_t) x->d1[0]
  + ((uint32_t) x->d1[1] << 8)
  + ((uint32_t) x->d1[2] << 16)
  + ((uint32_t) x->d1[3] << 24));
  else
    {
      int32_t d;
      ((char *) &d)[0] = x->d1[0];
      ((char *) &d)[1] = x->d1[1];
      ((char *) &d)[2] = x->d1[2];
      ((char *) &d)[3] = x->d1[3];
      c = d;
    }
  b.b4 = 0;
  b.b1 = c / 10000L % 10000;
  b.b2 = c / 100 % 100;
  b.b3 = c % 100;
  foo (0, &b, y);
  return y;
}

int
main (void)
{
  uint32_t x = 900070503;
  unsigned int y = 2;
  struct C c = { 0 }, *cptr = &c;
  struct D d = { (char *) &x, &cptr, 0 };
  if (test (&d, &y) != &y)
    abort ();
  return 0;
}

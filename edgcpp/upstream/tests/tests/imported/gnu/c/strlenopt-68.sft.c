//type: rp
//options: 
# 0 "./strlenopt-68.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-68.c"
# 9 "./strlenopt-68.c"
# 1 "./strlenopt.h" 1





typedef long unsigned int size_t;
extern void abort (void);
void *calloc (size_t, size_t);
void *malloc (size_t);
void free (void *);
char *strdup (const char *);
size_t strlen (const char *);
size_t strnlen (const char *, size_t);
void *memcpy (void *__restrict, const void *__restrict, size_t);
void *memmove (void *, const void *, size_t);
char *strcpy (char *__restrict, const char *__restrict);
char *strcat (char *__restrict, const char *__restrict);
char *strchr (const char *, int);
int strcmp (const char *, const char *);
int strncmp (const char *, const char *, size_t);
void *memset (void *, int, size_t);
int memcmp (const void *, const void *, size_t);
int strcmp (const char *, const char *);





int sprintf (char * __restrict, const char *__restrict, ...);
int snprintf (char * __restrict, size_t, const char *__restrict, ...);
# 10 "./strlenopt-68.c" 2
# 18 "./strlenopt-68.c"
const char gs0[] = "";
const char gs3[] = "123";

char gc;
char ga5[7];

struct S { char n, ma7[7], max[]; };


__attribute__ ((noclone, noinline, noipa)) void
equal_4_gs0_gs3_ga5_m1 (int i)
{
  strcpy (ga5, "1234");
  const char *p = i < 0 ? gs0 : 0 < i ? gs3 : ga5;

  ((snprintf (0, 0, "%s", p) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 33, "snprintf (0, 0, \"%s\", p) == 0"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
equal_4_gs0_gs3_ga5_0 (int i)
{
  strcpy (ga5, "1234");
  const char *p = i < 0 ? gs0 : 0 < i ? gs3 : ga5;

  ((snprintf (0, 0, "%s", p) == 4) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 42, "snprintf (0, 0, \"%s\", p) == 4"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
equal_4_gs0_gs3_ga5_p1 (int i)
{
  strcpy (ga5, "1234");
  const char *p = i < 0 ? gs0 : 0 < i ? gs3 : ga5;

  ((snprintf (0, 0, "%s", p) == 3) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 51, "snprintf (0, 0, \"%s\", p) == 3"), __builtin_abort ()));
}


__attribute__ ((noclone, noinline, noipa)) void
equal_4_gs0_ga5_gs3_m1 (int i)
{
  strcpy (ga5, "1234");
  const char *p = i < 0 ? gs0 : 0 < i ? ga5 : gs3;

  ((snprintf (0, 0, "%s", p) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 61, "snprintf (0, 0, \"%s\", p) == 0"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
equal_4_gs0_ga5_gs3_0 (int i)
{
  strcpy (ga5, "1234");
  const char *p = i < 0 ? gs0 : 0 < i ? ga5 : gs3;

  ((snprintf (0, 0, "%s", p) == 3) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 70, "snprintf (0, 0, \"%s\", p) == 3"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
equal_4_gs0_ga5_gs3_p1 (int i)
{
  strcpy (ga5, "1234");
  const char *p = i < 0 ? gs0 : 0 < i ? ga5 : gs3;

  ((snprintf (0, 0, "%s", p) == 4) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 79, "snprintf (0, 0, \"%s\", p) == 4"), __builtin_abort ()));
}


__attribute__ ((noclone, noinline, noipa)) void
equal_4_ga5_gs0_gs3_m1 (int i)
{
  strcpy (ga5, "1234");
  const char *p = i < 0 ? ga5 : 0 < i ? gs0 : gs3;

  ((snprintf (0, 0, "%s", p) == 4) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 89, "snprintf (0, 0, \"%s\", p) == 4"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
equal_4_ga5_gs0_gs3_0 (int i)
{
  strcpy (ga5, "1234");
  const char *p = i < 0 ? ga5 : 0 < i ? gs0 : gs3;

  ((snprintf (0, 0, "%s", p) == 3) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 98, "snprintf (0, 0, \"%s\", p) == 3"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
equal_4_ga5_gs0_gs3_p1 (int i)
{
  strcpy (ga5, "1234");
  const char *p = i < 0 ? ga5 : 0 < i ? gs0 : gs3;

  ((snprintf (0, 0, "%s", p) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 107, "snprintf (0, 0, \"%s\", p) == 0"), __builtin_abort ()));
}


__attribute__ ((noclone, noinline, noipa)) void
equal_4_ga5_gs3_gs0_m1 (int i)
{
  strcpy (ga5, "1234");
  const char *p = i < 0 ? ga5 : 0 < i ? gs3 : gs0;

  ((snprintf (0, 0, "%s", p) == 4) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 117, "snprintf (0, 0, \"%s\", p) == 4"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
equal_4_ga5_gs3_gs0_0 (int i)
{
  strcpy (ga5, "1234");
  const char *p = i < 0 ? ga5 : 0 < i ? gs3 : gs0;

  ((snprintf (0, 0, "%s", p) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 126, "snprintf (0, 0, \"%s\", p) == 0"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
equal_4_ga5_gs3_gs0_p1 (int i)
{
  strcpy (ga5, "1234");
  const char *p = i < 0 ? ga5 : 0 < i ? gs3 : gs0;

  ((snprintf (0, 0, "%s", p) == 3) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 135, "snprintf (0, 0, \"%s\", p) == 3"), __builtin_abort ()));
}


__attribute__ ((noclone, noinline, noipa)) void
equal_4_gs3_gs0_ga5_m1 (int i)
{
  strcpy (ga5, "1234");
  const char *p = i < 0 ? gs3 : 0 < i ? gs0 : ga5;

  ((snprintf (0, 0, "%s", p) == 3) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 145, "snprintf (0, 0, \"%s\", p) == 3"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
equal_4_gs3_gs0_ga5_0 (int i)
{
  strcpy (ga5, "1234");
  const char *p = i < 0 ? gs3 : 0 < i ? gs0 : ga5;

  ((snprintf (0, 0, "%s", p) == 4) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 154, "snprintf (0, 0, \"%s\", p) == 4"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
equal_4_gs3_gs0_ga5_p1 (int i)
{
  strcpy (ga5, "1234");
  const char *p = i < 0 ? gs3 : 0 < i ? gs0 : ga5;

  ((snprintf (0, 0, "%s", p) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 163, "snprintf (0, 0, \"%s\", p) == 0"), __builtin_abort ()));
}





__attribute__ ((noclone, noinline, noipa)) void
min_4_gc_gs3_ga5_m1 (int i)
{
  gc = 0;
  memcpy (ga5, "1234", 4);
  const char *p = i < 0 ? &gc : 0 < i ? gs3 : ga5;

  ((snprintf (0, 0, "%s", p) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 177, "snprintf (0, 0, \"%s\", p) == 0"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
min_4_gc_gs3_ga5_0 (int i)
{
  gc = 0;
  memcpy (ga5, "1234", 4);
  const char *p = i < 0 ? &gc : 0 < i ? gs3 : ga5;

  ((snprintf (0, 0, "%s", p) == 4) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 187, "snprintf (0, 0, \"%s\", p) == 4"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
min_4_gc_gs3_ga5_p1 (int i)
{
  gc = 0;
  memcpy (ga5, "1234", 4);
  const char *p = i < 0 ? &gc : 0 < i ? gs3 : ga5;

  ((snprintf (0, 0, "%s", p) == 3) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 197, "snprintf (0, 0, \"%s\", p) == 3"), __builtin_abort ()));
}


__attribute__ ((noclone, noinline, noipa)) void
min_4_gc_ga5_gs3_m1 (int i)
{
  gc = 0;
  memcpy (ga5, "1234", 4);
  const char *p = i < 0 ? &gc : 0 < i ? ga5 : gs3;

  ((snprintf (0, 0, "%s", p) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 208, "snprintf (0, 0, \"%s\", p) == 0"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
min_4_gc_ga5_gs3_0 (int i)
{
  gc = 0;
  memcpy (ga5, "1234", 4);
  const char *p = i < 0 ? &gc : 0 < i ? ga5 : gs3;

  ((snprintf (0, 0, "%s", p) == 3) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 218, "snprintf (0, 0, \"%s\", p) == 3"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
min_4_gc_ga5_gs3_p1 (int i)
{
  gc = 0;
  memcpy (ga5, "1234", 4);
  const char *p = i < 0 ? &gc : 0 < i ? ga5 : gs3;

  ((snprintf (0, 0, "%s", p) == 4) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 228, "snprintf (0, 0, \"%s\", p) == 4"), __builtin_abort ()));
}


__attribute__ ((noclone, noinline, noipa)) void
min_4_ga5_gc_gs3_m1 (int i)
{
  gc = 0;
  memcpy (ga5, "1234", 4);
  const char *p = i < 0 ? ga5 : 0 < i ? &gc : gs3;

  ((snprintf (0, 0, "%s", p) == 4) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 239, "snprintf (0, 0, \"%s\", p) == 4"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
min_4_ga5_gc_gs3_0 (int i)
{
  gc = 0;
  memcpy (ga5, "1234", 4);
  const char *p = i < 0 ? ga5 : 0 < i ? &gc : gs3;

  ((snprintf (0, 0, "%s", p) == 3) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 249, "snprintf (0, 0, \"%s\", p) == 3"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
min_4_ga5_gc_gs3_p1 (int i)
{
  gc = 0;
  memcpy (ga5, "1234", 4);
  const char *p = i < 0 ? ga5 : 0 < i ? &gc : gs3;

  ((snprintf (0, 0, "%s", p) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 259, "snprintf (0, 0, \"%s\", p) == 0"), __builtin_abort ()));
}


__attribute__ ((noclone, noinline, noipa)) void
min_4_ga5_gs3_gc_m1 (int i)
{
  gc = 0;
  memcpy (ga5, "1234", 4);
  const char *p = i < 0 ? ga5 : 0 < i ? gs3 : &gc;

  ((snprintf (0, 0, "%s", p) == 4) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 270, "snprintf (0, 0, \"%s\", p) == 4"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
min_4_ga5_gs3_gc_0 (int i)
{
  gc = 0;
  memcpy (ga5, "1234", 4);
  const char *p = i < 0 ? ga5 : 0 < i ? gs3 : &gc;

  ((snprintf (0, 0, "%s", p) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 280, "snprintf (0, 0, \"%s\", p) == 0"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
min_4_ga5_gs3_gc_p1 (int i)
{
  gc = 0;
  memcpy (ga5, "1234", 4);
  const char *p = i < 0 ? ga5 : 0 < i ? gs3 : &gc;

  ((snprintf (0, 0, "%s", p) == 3) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 290, "snprintf (0, 0, \"%s\", p) == 3"), __builtin_abort ()));
}


__attribute__ ((noclone, noinline, noipa)) void
min_4_gs3_gc_ga5_m1 (int i)
{
  gc = 0;
  memcpy (ga5, "1234", 4);
  const char *p = i < 0 ? gs3 : 0 < i ? &gc : ga5;

  ((snprintf (0, 0, "%s", p) == 3) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 301, "snprintf (0, 0, \"%s\", p) == 3"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
min_4_gs3_gc_ga5_0 (int i)
{
  gc = 0;
  memcpy (ga5, "1234", 4);
  const char *p = i < 0 ? gs3 : 0 < i ? &gc : ga5;

  ((snprintf (0, 0, "%s", p) == 4) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 311, "snprintf (0, 0, \"%s\", p) == 4"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
min_4_gs3_gc_ga5_p1 (int i)
{
  gc = 0;
  memcpy (ga5, "1234", 4);
  const char *p = i < 0 ? gs3 : 0 < i ? &gc : ga5;

  ((snprintf (0, 0, "%s", p) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 321, "snprintf (0, 0, \"%s\", p) == 0"), __builtin_abort ()));
}


int main (void)
{
  equal_4_gs0_gs3_ga5_m1 (-1);
  equal_4_gs0_gs3_ga5_0 ( 0);
  equal_4_gs0_gs3_ga5_p1 (+1);

  equal_4_gs0_ga5_gs3_m1 (-1);
  equal_4_gs0_ga5_gs3_0 ( 0);
  equal_4_gs0_ga5_gs3_p1 (+1);

  equal_4_ga5_gs0_gs3_m1 (-1);
  equal_4_ga5_gs0_gs3_0 ( 0);
  equal_4_ga5_gs0_gs3_p1 (+1);

  equal_4_ga5_gs3_gs0_m1 (-1);
  equal_4_ga5_gs3_gs0_0 ( 0);
  equal_4_ga5_gs3_gs0_p1 (+1);

  equal_4_gs3_gs0_ga5_m1 (-1);
  equal_4_gs3_gs0_ga5_0 ( 0);
  equal_4_gs3_gs0_ga5_p1 (+1);



  memset (ga5, 0, sizeof ga5);
  min_4_gc_gs3_ga5_m1 (-1);
  memset (ga5, 0, sizeof ga5);
  min_4_gc_gs3_ga5_0 ( 0);
  memset (ga5, 0, sizeof ga5);
  min_4_gc_gs3_ga5_p1 (+1);

  memset (ga5, 0, sizeof ga5);
  min_4_gc_ga5_gs3_m1 (-1);
  memset (ga5, 0, sizeof ga5);
  min_4_gc_ga5_gs3_0 ( 0);
  memset (ga5, 0, sizeof ga5);
  min_4_gc_ga5_gs3_p1 (+1);

  memset (ga5, 0, sizeof ga5);
  min_4_ga5_gc_gs3_m1 (-1);
  memset (ga5, 0, sizeof ga5);
  min_4_ga5_gc_gs3_0 ( 0);
  memset (ga5, 0, sizeof ga5);
  min_4_ga5_gc_gs3_p1 (+1);

  memset (ga5, 0, sizeof ga5);
  min_4_ga5_gs3_gc_m1 (-1);
  memset (ga5, 0, sizeof ga5);
  min_4_ga5_gs3_gc_0 ( 0);
  memset (ga5, 0, sizeof ga5);
  min_4_ga5_gs3_gc_p1 (+1);

  memset (ga5, 0, sizeof ga5);
  min_4_gs3_gc_ga5_m1 (-1);
  memset (ga5, 0, sizeof ga5);
  min_4_gs3_gc_ga5_0 ( 0);
  memset (ga5, 0, sizeof ga5);
  min_4_gs3_gc_ga5_p1 (+1);
}

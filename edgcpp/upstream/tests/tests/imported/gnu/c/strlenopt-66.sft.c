//type: rp
//options: 
# 0 "./strlenopt-66.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-66.c"





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
# 7 "./strlenopt-66.c" 2
# 16 "./strlenopt-66.c"
__attribute__ ((noclone, noinline, noipa)) void
clobber (void *p, int x, size_t n)
{
  for (volatile unsigned char *q = p; n--; )
    *q = x;
}

__attribute__ ((noclone, noinline, noipa)) void
test_strcmp (void)
{
  char a[8], b[8];
  strcpy (a, "1235");
  strcpy (b, "1234");

  ((strcmp (a, b)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 30, "strcmp (a, b)"), __builtin_abort ()));

  clobber (a, 0, sizeof a);
  clobber (b, 0, sizeof b);
  clobber (b + 4, '5', 1);

  memcpy (a, "1234", 4);
  memcpy (b, "1234", 4);

  ((0 > strcmp (a, b)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 39, "0 > strcmp (a, b)"), __builtin_abort ()));
  ((0 < strcmp (b, a)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 40, "0 < strcmp (b, a)"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
test_strncmp (void)
{
  char a[8], b[8];
  strcpy (a, "1235");
  strcpy (b, "1234");

  ((0 == strncmp (a, b, 1)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 50, "0 == strncmp (a, b, 1)"), __builtin_abort ()));
  ((0 == strncmp (a, b, 2)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 51, "0 == strncmp (a, b, 2)"), __builtin_abort ()));
  ((0 == strncmp (a, b, 3)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 52, "0 == strncmp (a, b, 3)"), __builtin_abort ()));
  ((0 < strncmp (a, b, 4)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 53, "0 < strncmp (a, b, 4)"), __builtin_abort ()));
  ((0 > strncmp (b, a, 4)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 54, "0 > strncmp (b, a, 4)"), __builtin_abort ()));

  clobber (a, 0, sizeof a);
  clobber (b, 0, sizeof b);
  clobber (b + 4, '5', 1);

  memcpy (a, "1234", 4);
  memcpy (b, "1234", 4);

  ((0 == strncmp (a, b, 4)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 63, "0 == strncmp (a, b, 4)"), __builtin_abort ()));
  ((0 > strncmp (a, b, 5)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 64, "0 > strncmp (a, b, 5)"), __builtin_abort ()));
  ((0 < strncmp (b, a, 5)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 65, "0 < strncmp (b, a, 5)"), __builtin_abort ()));
}


__attribute__ ((noclone, noinline, noipa)) void
test_strncmp_a4_cond_s5_s2_2 (const char *s, int i)
{
  char a4[4];
  strcpy (a4, s);
  ((0 == strncmp (a4, i ? "12345" : "12", 2)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 74, "0 == strncmp (a4, i ? \"12345\" : \"12\", 2)"), __builtin_abort ()));
}


__attribute__ ((noclone, noinline, noipa)) void
test_strncmp_a4_cond_a5_s2_5 (const char *s, const char *t, int i)
{
  char a4[4], a5[5];
  strcpy (a4, s);
  strcpy (a5, t);
  ((0 == strncmp (a4, i ? a5 : "12", 5)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 84, "0 == strncmp (a4, i ? a5 : \"12\", 5)"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void
test_strncmp_a4_cond_a5_a3_n (const char *s1, const char *s2, const char *s3,
         int i, unsigned n)
{
  char a3[3], a4[4], a5[5];
  strcpy (a3, s1);
  strcpy (a4, s2);
  strcpy (a5, s3);
  ((0 == strncmp (a4, i ? a5 : a3, n)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 95, "0 == strncmp (a4, i ? a5 : a3, n)"), __builtin_abort ()));
}


int main (void)
{
  test_strcmp ();
  test_strncmp ();
  test_strncmp_a4_cond_s5_s2_2 ("12", 0);
  test_strncmp_a4_cond_a5_s2_5 ("12", "1234", 0);

  test_strncmp_a4_cond_a5_a3_n ("12", "1", "1", 0, 1);
  test_strncmp_a4_cond_a5_a3_n ("", "1", "1234", 1, 1);

  test_strncmp_a4_cond_a5_a3_n ("12", "12", "1", 0, 2);
  test_strncmp_a4_cond_a5_a3_n ("", "12", "1234", 1, 2);

  test_strncmp_a4_cond_a5_a3_n ("12", "123", "1", 0, 2);
  test_strncmp_a4_cond_a5_a3_n ("", "123", "1234", 1, 3);
}

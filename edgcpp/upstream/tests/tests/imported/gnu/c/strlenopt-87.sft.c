//type: rp
//options: 
# 0 "./strlenopt-87.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-87.c"





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
# 7 "./strlenopt-87.c" 2


char a2[2], a3[3];


static inline __attribute__ ((always_inline)) int
verify_not_equal (const char *s, const char *t, int x)
{
  int n = x < 0 ? strlen (s) : 0 < x ? strlen (t) : strlen (s) + strlen (t);

  if (strcmp (t, s) == 0)
    abort ();

  return n;
}

__attribute__ ((noipa)) int test_a2_s (const char *s)
{
  return verify_not_equal (a2, s, 0);
}

__attribute__ ((noipa)) int test_a2_a3 (void)
{
  return verify_not_equal (a2, a3, 0);
}

__attribute__ ((noipa)) int test_a3_a2 (void)
{
  return verify_not_equal (a3, a2, 0);
}

__attribute__ ((noipa)) int test_s_a2 (const char *s)
{
  return verify_not_equal (s, a2, 0);
}


__attribute__ ((noipa)) int test_a2_s_1 (const char *s)
{
  return verify_not_equal (a2, s, -1);
}

__attribute__ ((noipa)) int test_a2_a3_1 (void)
{
  return verify_not_equal (a2, a3, -1);
}

__attribute__ ((noipa)) int test_a3_a2_1 (void)
{
  return verify_not_equal (a3, a2, -1);
}

__attribute__ ((noipa)) int test_s_a2_1 (const char *s)
{
  return verify_not_equal (s, a2, -1);
}


__attribute__ ((noipa)) int test_a2_s_2 (const char *s)
{
  return verify_not_equal (a2, s, +1);
}

__attribute__ ((noipa)) int test_a2_a3_2 (void)
{
  return verify_not_equal (a2, a3, +1);
}

__attribute__ ((noipa)) int test_a3_a2_2 (void)
{
  return verify_not_equal (a3, a2, +1);
}

__attribute__ ((noipa)) int test_s_a2_2 (const char *s)
{
  return verify_not_equal (s, a2, +1);
}

int main (void)
{
  a2[0] = '1';
  a3[0] = '1';
  a3[0] = '2';

  test_a2_s ("");
  test_a2_a3 ();
  test_a3_a2 ();
  test_s_a2 ("");

  test_a2_s_1 ("");
  test_a2_a3_1 ();
  test_a3_a2_1 ();
  test_s_a2_1 ("");

  test_a2_s_2 ("");
  test_a2_a3_2 ();
  test_a3_a2_2 ();
  test_s_a2_2 ("");
}

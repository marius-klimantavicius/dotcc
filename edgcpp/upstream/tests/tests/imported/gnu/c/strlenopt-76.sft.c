//type: rp
//options: 
# 0 "./strlenopt-76.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-76.c"





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
# 7 "./strlenopt-76.c" 2
# 17 "./strlenopt-76.c"
int i = 0;

const char s[] = "1234567";

char a[32];

__attribute__ ((noclone, noinline, noipa)) void lower_bound_assign_into_empty (void)
{
  a[0] = '1';
  a[1] = '2';
  a[2] = '3';
  ((strlen (a) == 3) ? (void)0 : (__builtin_printf ("line %i %s: assertion failed: %s\n", 28, __func__, "strlen (a) == 3"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void lower_bound_assign_into_longest (void)
{
  a[0] = '1';
  a[1] = '2';
  a[2] = '3';
  ((strlen (a) == 31) ? (void)0 : (__builtin_printf ("line %i %s: assertion failed: %s\n", 36, __func__, "strlen (a) == 31"), __builtin_abort ()));
}


__attribute__ ((noclone, noinline, noipa)) void lower_bound_assign_into_empty_idx_3 (int idx)
{
  a[0] = '1';
  a[1] = '2';
  a[2] = '3';
  a[idx] = 'x';
  ((strlen (a) == 4) ? (void)0 : (__builtin_printf ("line %i %s: assertion failed: %s\n", 46, __func__, "strlen (a) == 4"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void lower_bound_assign_into_longest_idx_2 (int idx)
{
  a[0] = '1';
  a[1] = '2';
  a[2] = '3';
  a[idx] = '\0';
  ((strlen (a) == 2) ? (void)0 : (__builtin_printf ("line %i %s: assertion failed: %s\n", 55, __func__, "strlen (a) == 2"), __builtin_abort ()));
}


__attribute__ ((noclone, noinline, noipa)) void lower_bound_memcpy_into_empty (void)
{
  memcpy (a, "123", 3);
  ((strlen (a) == 3) ? (void)0 : (__builtin_printf ("line %i %s: assertion failed: %s\n", 62, __func__, "strlen (a) == 3"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void lower_bound_memcpy_into_longest (void)
{
  memcpy (a, "123", 3);
  ((strlen (a) == 31) ? (void)0 : (__builtin_printf ("line %i %s: assertion failed: %s\n", 68, __func__, "strlen (a) == 31"), __builtin_abort ()));
}


__attribute__ ((noclone, noinline, noipa)) void lower_bound_memcpy_memcpy_into_empty (void)
{
  memcpy (a, "123", 3);
  memcpy (a + 2, "345", 3);
  ((strlen (a) == 5) ? (void)0 : (__builtin_printf ("line %i %s: assertion failed: %s\n", 76, __func__, "strlen (a) == 5"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void lower_bound_memcpy_memcpy_into_longest (void)
{
  memcpy (a, "123", 3);
  memcpy (a + 2, "345", 3);
  ((strlen (a) == 31) ? (void)0 : (__builtin_printf ("line %i %s: assertion failed: %s\n", 83, __func__, "strlen (a) == 31"), __builtin_abort ()));
}


__attribute__ ((noclone, noinline, noipa)) void memove_forward_strlen (void)
{
  char a[] = "123456";

  memmove (a, a + 1, sizeof a - 1);

  ((strlen (a) == 5) ? (void)0 : (__builtin_printf ("line %i %s: assertion failed: %s\n", 93, __func__, "strlen (a) == 5"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void memove_backward_into_empty_strlen (void)
{
  strcpy (a, "123456");

  memmove (a + 1, a, 6);

  ((strlen (a) == 7) ? (void)0 : (__builtin_printf ("line %i %s: assertion failed: %s\n", 102, __func__, "strlen (a) == 7"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void memove_backward_into_longest_strlen (void)
{
  memcpy (a, "123456", 6);

  memmove (a + 1, a, 6);

  ((strlen (a) == 31) ? (void)0 : (__builtin_printf ("line %i %s: assertion failed: %s\n", 111, __func__, "strlen (a) == 31"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa)) void memove_strcmp (void)
{



  char a[] = "123456";
  char b[] = "000000";

  memmove (b, a, sizeof a);

  ((strlen (a) == 6) ? (void)0 : (__builtin_printf ("line %i %s: assertion failed: %s\n", 124, __func__, "strlen (a) == 6"), __builtin_abort ()));
  ((strlen (b) == 6) ? (void)0 : (__builtin_printf ("line %i %s: assertion failed: %s\n", 125, __func__, "strlen (b) == 6"), __builtin_abort ()));
  ((strcmp (a, b) == 0) ? (void)0 : (__builtin_printf ("line %i %s: assertion failed: %s\n", 126, __func__, "strcmp (a, b) == 0"), __builtin_abort ()));
}


int main (void)
{
  memset (a, '\0', sizeof a);
  lower_bound_assign_into_empty ();

  memset (a, 'x', sizeof a - 1);
  a[sizeof a - 1] = '\0';
  lower_bound_assign_into_longest ();

  memset (a, '\0', sizeof a);
  lower_bound_assign_into_empty_idx_3 (3);

  memset (a, 'x', sizeof a - 1);
  a[sizeof a - 1] = '\0';
  lower_bound_assign_into_longest_idx_2 (2);

  memset (a, '\0', sizeof a);
  lower_bound_memcpy_into_empty ();

  memset (a, 'x', sizeof a - 1);
  a[sizeof a - 1] = '\0';
  lower_bound_memcpy_into_longest ();

  memset (a, 'x', sizeof a - 1);
  a[sizeof a - 1] = '\0';
  lower_bound_memcpy_into_longest ();

  memset (a, '\0', sizeof a);
  lower_bound_memcpy_memcpy_into_empty ();

  memset (a, 'x', sizeof a - 1);
  a[sizeof a - 1] = '\0';
  lower_bound_memcpy_memcpy_into_longest ();

  memove_forward_strlen ();

  memset (a, '\0', sizeof a);
  memove_backward_into_empty_strlen ();

  memset (a, 'x', sizeof a - 1);
  a[sizeof a - 1] = '\0';
  memove_backward_into_longest_strlen ();

  memove_strcmp ();
}

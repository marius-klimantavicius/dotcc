//type: rp
//options: 
# 0 "./strlenopt-63.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-63.c"







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
# 9 "./strlenopt-63.c" 2
# 17 "./strlenopt-63.c"
typedef char A5[5];

A5 a5[5];
A5* p[5] = { &a5[4], &a5[3], &a5[2], &a5[1], &a5[0] };

__attribute__ ((noclone, noinline, noipa))
void deref_deref (void)
{
  strcpy (**p, "12345");
  ((strlen (**p) == 5) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 26, "strlen (**p) == 5"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa))
void deref_idx_0 (void)
{
  strcpy (*p[0], "");
  ((strlen (*p[0]) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 33, "strlen (*p[0]) == 0"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa))
void deref_idx_1 (void)
{
  strcpy (*p[1], "1");
  ((strlen (*p[1]) == 1) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 40, "strlen (*p[1]) == 1"), __builtin_abort ()));
  ((strlen (&(*p[1])[1]) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 41, "strlen (&(*p[1])[1]) == 0"), __builtin_abort ()));

  ((strlen (*p[0]) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 43, "strlen (*p[0]) == 0"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa))
void deref_idx_2 (void)
{
  strcpy (*p[2], "12");
  ((strlen (*p[2]) == 2) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 50, "strlen (*p[2]) == 2"), __builtin_abort ()));
  ((strlen (&(*p[2])[1]) == 1) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 51, "strlen (&(*p[2])[1]) == 1"), __builtin_abort ()));
  ((strlen (&(*p[2])[2]) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 52, "strlen (&(*p[2])[2]) == 0"), __builtin_abort ()));

  ((strlen (*p[1]) == 1) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 54, "strlen (*p[1]) == 1"), __builtin_abort ()));
  ((strlen (*p[0]) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 55, "strlen (*p[0]) == 0"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa))
void deref_idx_3 (void)
{
  strcpy (*p[3], "123");
  ((strlen (*p[3]) == 3) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 62, "strlen (*p[3]) == 3"), __builtin_abort ()));
  ((strlen (&(*p[3])[1]) == 2) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 63, "strlen (&(*p[3])[1]) == 2"), __builtin_abort ()));
  ((strlen (&(*p[3])[2]) == 1) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 64, "strlen (&(*p[3])[2]) == 1"), __builtin_abort ()));
  ((strlen (&(*p[3])[3]) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 65, "strlen (&(*p[3])[3]) == 0"), __builtin_abort ()));

  ((strlen (*p[2]) == 2) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 67, "strlen (*p[2]) == 2"), __builtin_abort ()));
  ((strlen (*p[1]) == 1) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 68, "strlen (*p[1]) == 1"), __builtin_abort ()));
  ((strlen (*p[0]) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 69, "strlen (*p[0]) == 0"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa))
void deref_idx_4 (void)
{
  strcpy (*p[4], "1234");
  ((strlen (*p[4]) == 4) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 76, "strlen (*p[4]) == 4"), __builtin_abort ()));
  ((strlen (&(*p[4])[1]) == 3) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 77, "strlen (&(*p[4])[1]) == 3"), __builtin_abort ()));
  ((strlen (&(*p[4])[2]) == 2) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 78, "strlen (&(*p[4])[2]) == 2"), __builtin_abort ()));
  ((strlen (&(*p[4])[3]) == 1) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 79, "strlen (&(*p[4])[3]) == 1"), __builtin_abort ()));
  ((strlen (&(*p[4])[4]) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 80, "strlen (&(*p[4])[4]) == 0"), __builtin_abort ()));

  ((strlen (*p[3]) == 3) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 82, "strlen (*p[3]) == 3"), __builtin_abort ()));
  ((strlen (*p[2]) == 2) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 83, "strlen (*p[2]) == 2"), __builtin_abort ()));
  ((strlen (*p[1]) == 1) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 84, "strlen (*p[1]) == 1"), __builtin_abort ()));
  ((strlen (*p[0]) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 85, "strlen (*p[0]) == 0"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa))
void deref_idx_4_x (void)
{
  strcpy (*p[4], "");
  ((strlen (*p[4]) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 92, "strlen (*p[4]) == 0"), __builtin_abort ()));
  ((strlen (*p[3]) == 3) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 93, "strlen (*p[3]) == 3"), __builtin_abort ()));
  ((strlen (*p[2]) == 2) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 94, "strlen (*p[2]) == 2"), __builtin_abort ()));
  ((strlen (*p[1]) == 1) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 95, "strlen (*p[1]) == 1"), __builtin_abort ()));
  ((strlen (*p[0]) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 96, "strlen (*p[0]) == 0"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa))
void deref_idx_3_x (void)
{
  strcpy (&(*p[3])[0], "1");
  ((strlen (*p[4]) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 103, "strlen (*p[4]) == 0"), __builtin_abort ()));
  ((strlen (*p[3]) == 1) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 104, "strlen (*p[3]) == 1"), __builtin_abort ()));
  ((strlen (*p[2]) == 2) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 105, "strlen (*p[2]) == 2"), __builtin_abort ()));
  ((strlen (*p[1]) == 1) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 106, "strlen (*p[1]) == 1"), __builtin_abort ()));
  ((strlen (*p[0]) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 107, "strlen (*p[0]) == 0"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa))
void deref_idx_2_x (void)
{
  strcpy (*p[2], "12");
  ((strlen (*p[4]) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 114, "strlen (*p[4]) == 0"), __builtin_abort ()));
  ((strlen (*p[3]) == 1) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 115, "strlen (*p[3]) == 1"), __builtin_abort ()));
  ((strlen (*p[2]) == 2) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 116, "strlen (*p[2]) == 2"), __builtin_abort ()));
  ((strlen (*p[1]) == 1) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 117, "strlen (*p[1]) == 1"), __builtin_abort ()));
  ((strlen (*p[0]) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 118, "strlen (*p[0]) == 0"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa))
void deref_idx_1_x (void)
{
  strcpy (*p[1], "123");
  ((strlen (*p[4]) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 125, "strlen (*p[4]) == 0"), __builtin_abort ()));
  ((strlen (*p[3]) == 1) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 126, "strlen (*p[3]) == 1"), __builtin_abort ()));
  ((strlen (*p[2]) == 2) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 127, "strlen (*p[2]) == 2"), __builtin_abort ()));
  ((strlen (*p[1]) == 3) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 128, "strlen (*p[1]) == 3"), __builtin_abort ()));
  ((strlen (*p[0]) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 129, "strlen (*p[0]) == 0"), __builtin_abort ()));
}

__attribute__ ((noclone, noinline, noipa))
void deref_idx_0_x (void)
{
  strcpy (*p[0], "1234");
  ((strlen (*p[4]) == 0) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 136, "strlen (*p[4]) == 0"), __builtin_abort ()));
  ((strlen (*p[3]) == 1) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 137, "strlen (*p[3]) == 1"), __builtin_abort ()));
  ((strlen (*p[2]) == 2) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 138, "strlen (*p[2]) == 2"), __builtin_abort ()));
  ((strlen (*p[1]) == 3) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 139, "strlen (*p[1]) == 3"), __builtin_abort ()));
  ((strlen (*p[0]) == 4) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 140, "strlen (*p[0]) == 4"), __builtin_abort ()));
}

int main (void)
{
  deref_deref ();

  deref_idx_0 ();
  deref_idx_1 ();
  deref_idx_2 ();
  deref_idx_3 ();
  deref_idx_4 ();

  deref_idx_4_x ();
  deref_idx_3_x ();
  deref_idx_2_x ();
  deref_idx_1_x ();
  deref_idx_0_x ();
}

//type: rp
//options: 
# 0 "./strlenopt-46.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-46.c"




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
# 6 "./strlenopt-46.c" 2



char a[] = "12345";

__attribute__ ((noipa)) void f0 (void)
{
  unsigned n0 = strnlen (a, 0);
  unsigned n1 = strlen (a);

  if (n0 != 0 || n1 != 5)
    abort ();
}

__attribute__ ((noipa)) void f1 (void)
{
  unsigned n0 = strnlen (a, 1);
  unsigned n1 = strlen (a);

  if (n0 != 1 || n1 != 5)
    abort ();
}

__attribute__ ((noipa)) void f2 (void)
{
  unsigned n0 = strnlen (a, 2);
  unsigned n1 = strlen (a);

  if (n0 != 2 || n1 != 5)
    abort ();
}

__attribute__ ((noipa)) void f3 (void)
{
  unsigned n0 = strnlen (a, 3);
  unsigned n1 = strlen (a);

  if (n0 != 3 || n1 != 5)
    abort ();
}

__attribute__ ((noipa)) void f4 (void)
{
  unsigned n0 = strnlen (a, 4);
  unsigned n1 = strlen (a);

  if (n0 != 4 || n1 != 5)
    abort ();
}

__attribute__ ((noipa)) void f5 (void)
{
  unsigned n0 = strnlen (a, 5);
  unsigned n1 = strlen (a);

  if (n0 != 5 || n1 != 5)
    abort ();
}

__attribute__ ((noipa)) void f6 (void)
{
  unsigned n0 = strnlen (a, 6);
  unsigned n1 = strlen (a);

  if (n0 != 5 || n1 != 5)
    abort ();
}

__attribute__ ((noipa)) void fx (unsigned n)
{
  unsigned n0 = strnlen (a, n);
  unsigned n1 = strlen (a);

  unsigned min = n < 5 ? n : 5;
  if (n0 != min || n1 != 5)
    abort ();
}

__attribute__ ((noipa)) void g2 (void)
{
  strcpy (a, "123");
  unsigned n0 = strnlen (a, 2);
  unsigned n1 = strlen (a);

  if (n0 != 2 || n1 != 3)
    abort ();
}

__attribute__ ((noipa)) void g7 (void)
{
  strcpy (a, "123");
  unsigned n0 = strnlen (a, 7);
  unsigned n1 = strlen (a);

  if (n0 != 3 || n1 != 3)
    abort ();
}

__attribute__ ((noipa)) void gx (unsigned n)
{
  strcpy (a, "123");
  unsigned n0 = strnlen (a, n);
  unsigned n1 = strlen (a);

  unsigned min = n < 3 ? n : 3;
  if (n0 != min || n1 != 3)
    abort ();
}

int main (void)
{
  f0 ();
  f1 ();
  f2 ();
  f3 ();
  f4 ();
  f5 ();
  f6 ();
  fx (2);
  fx (7);

  g2 ();
  g7 ();
  gx (2);
  gx (7);
}




__attribute__ ((noipa)) size_t
strnlen (const char *s, size_t n)
{
  size_t len = 0;
  while (*s++ && n--)
    ++len;
  return len;
}

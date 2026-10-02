//type: rp
//options: 
# 0 "./strlenopt-9.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-9.c"



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
# 5 "./strlenopt-9.c" 2

__attribute__((noinline, noclone)) char *
fn1 (int r)
{
  char *p = r ? "a" : "bc";


  return strchr (p, '\0');
}

__attribute__((noinline, noclone)) size_t
fn2 (int r)
{
  char *p, q[10];
  strcpy (q, "abc");
  p = r ? "a" : q;


  return strlen (p);
}

__attribute__((noinline, noclone)) size_t
fn3 (char *p, int n)
{
  int i;
  p = strchr (p, '\0');

  strcat (p, "abcd");
  for (i = 0; i < n; i++)
    if ((i % 123) == 53)

      strcat (p, "efg");


  return strlen (p);
}

char buf[64];

__attribute__((noinline, noclone)) size_t
fn4 (char *x, int n)
{
  int i;
  size_t l;
  char a[64];
  char *p = strchr (x, '\0');

  strcpy (a, x);

  strcat (p, "abcd");
  for (i = 0; i < n; i++)
    if ((i % 123) == 53)

      strcat (a, "efg");

  l = strlen (p);

  strcpy (buf, a);
  return l;
}

int
main ()
{
  volatile int l = 1;
  char b[64];

  if (memcmp (fn1 (l) - 1, "a", 2) != 0)
    abort ();
  if (memcmp (fn1 (!l) - 2, "bc", 3) != 0)
    abort ();
  if (fn2 (l) != 1 || fn2 (!l) != 3)
    abort ();
  memset (b, '\0', sizeof b);
  memset (b, 'a', 3);
  if (fn3 (b, 10) != 4 || memcmp (b, "aaaabcd", 8) != 0)
    abort ();
  if (fn3 (b, 128) != 7 || memcmp (b, "aaaabcdabcdefg", 15) != 0)
    abort ();
  if (fn3 (b, 256) != 10 || memcmp (b, "aaaabcdabcdefgabcdefgefg", 25) != 0)
    abort ();
  if (fn4 (b, 10) != 4
      || memcmp (b, "aaaabcdabcdefgabcdefgefgabcd", 29) != 0
      || memcmp (buf, "aaaabcdabcdefgabcdefgefg", 25) != 0)
    abort ();
  if (fn4 (b, 128) != 4
      || memcmp (b, "aaaabcdabcdefgabcdefgefgabcdabcd", 33) != 0
      || memcmp (buf, "aaaabcdabcdefgabcdefgefgabcdefg", 32) != 0)
    abort ();
  if (fn4 (b, 256) != 4
      || memcmp (b, "aaaabcdabcdefgabcdefgefgabcdabcdabcd", 37) != 0
      || memcmp (buf, "aaaabcdabcdefgabcdefgefgabcdabcdefgefg", 39) != 0)
    abort ();
  return 0;
}

//type: rp
//options: 
# 0 "./strlenopt-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-3.c"



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
# 5 "./strlenopt-3.c" 2

__attribute__((noinline, noclone)) size_t
fn1 (char *p, char *q)
{
  size_t s = strlen (q);
  strcpy (p, q);
  return s - strlen (p);
}

__attribute__((noinline, noclone)) size_t
fn2 (char *p, char *q)
{
  size_t s = strlen (q);
  memcpy (p, q, s + 1);
  return s - strlen (p);
}

__attribute__((noinline, noclone)) size_t
fn3 (char *p)
{
  memcpy (p, "abcd", 5);
  return strlen (p);
}

__attribute__((noinline, noclone)) size_t
fn4 (char *p)
{
  memcpy (p, "efg\0hij", 6);
  return strlen (p);
}

int
main ()
{
  char buf[64];
  char *volatile p = buf;
  char *volatile q = "ABCDEF";
  buf[7] = 'G';
  if (fn1 (p, q) != 0 || memcmp (buf, "ABCDEF\0G", 8))
    abort ();
  q = "HIJ";
  if (fn2 (p + 1, q) != 0 || memcmp (buf, "AHIJ\0F\0G", 8))
    abort ();
  buf[6] = 'K';
  if (fn3 (p + 1) != 4 || memcmp (buf, "Aabcd\0KG", 8))
    abort ();
  if (fn4 (p) != 3 || memcmp (buf, "efg\0hiKG", 8))
    abort ();
  return 0;
}

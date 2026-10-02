//type: rp
//options: 
# 0 "./strlenopt-4g.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-4g.c"





# 1 "./strlenopt-4.c" 1



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

void *mempcpy (void *__restrict, const void *__restrict, size_t);
char *stpcpy (char *__restrict, const char *__restrict);


int sprintf (char * __restrict, const char *__restrict, ...);
int snprintf (char * __restrict, size_t, const char *__restrict, ...);
# 5 "./strlenopt-4.c" 2






__attribute__((noinline, noclone)) void
foo (char *p, const char *q, const char *r)
{
  strcpy (p, q);
  strcat (p, r);
  strcat (p, "abcd");
}





__attribute__((noinline, noclone)) void
bar (char *p, const char *q, const char *r)
{
  strcpy (p, "abcd");
  strcat (p, q);
  strcat (p, r);
}







__attribute__((noinline, noclone)) void
baz (char *p, const char *q, const char *r)
{
  strcat (p, q);
  strcat (p, "abcd");
  strcat (p, r);
  strcat (p, "efgh");
}

char buf[64];

int
main ()
{
  char *volatile p = buf;
  const char *volatile q = "ij";
  const char *volatile r = "klmno";
  foo (p, q, r);
  if (memcmp (buf, "ijklmnoabcd\0\0\0\0\0\0\0\0", 20) != 0)
    abort ();
  memset (buf, '\0', sizeof buf);
  bar (p, q, r);
  if (memcmp (buf, "abcdijklmno\0\0\0\0\0\0\0\0", 20) != 0)
    abort ();
  memset (buf, 'v', 3);
  memset (buf + 3, '\0', -3 + sizeof buf);
  baz (p, q, r);
  if (memcmp (buf, "vvvijabcdklmnoefgh\0", 20) != 0)
    abort ();
  return 0;
}
# 7 "./strlenopt-4g.c" 2

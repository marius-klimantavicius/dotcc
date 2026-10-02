//type: rp
//options: 
# 0 "./strlenopt-33g.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-33g.c"




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
# 6 "./strlenopt-33g.c" 2

volatile int v;

size_t __attribute__ ((noinline, noclone))
f1 (char *b)
{
  char a[30];
  v += 1;

  strcpy (a, b);
  int len1 = strlen (a);
  a[0] = '_';
  a[1] = 0;
  return len1 + strlen (a);
}

size_t __attribute__ ((noinline, noclone))
f2 (char *a, char *b)
{
  v += 2;

  strcpy (a, b);
  int len1 = strlen (a);
  a[0] = '_';
  a[1] = 0;
  return len1 + strlen (a);
}

int
main ()
{
  char a[30];
  if (f1 ("foo") != 4 || f2 (a, "foobar") != 7)
    abort ();
  return 0;
}

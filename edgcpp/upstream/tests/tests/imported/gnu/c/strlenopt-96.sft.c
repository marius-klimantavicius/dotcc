//type: fp
//options: 
# 0 "./strlenopt-96.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-96.c"






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
# 8 "./strlenopt-96.c" 2

typedef unsigned int V __attribute__((vector_size (2 * sizeof (int))));
typedef unsigned int W __attribute__((vector_size (4 * sizeof (int))));

size_t
foo (void)
{
  char a[64];
  *(long long *) a = 0x12003456789abcdeULL;
  return strlen (a);
}

size_t
bar (void)
{
  char a[64];
  *(V *) a = (V) { 0x12345678U, 0x9a00bcdeU };
  return strlen (a);
}

size_t
baz (unsigned int x)
{
  char a[64];
  *(V *) a = (V) { 0x12005678U, x };
  return strlen (a);
}

size_t
qux (unsigned int x)
{
  char a[64];
  *(W *)a = (W) { 0x12345678U, 0x9abcdef0U, 0x12005678U, x };
  return strlen (a);
}

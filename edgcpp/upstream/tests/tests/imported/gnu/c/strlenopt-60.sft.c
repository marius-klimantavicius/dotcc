//type: fp
//options: 
# 0 "./strlenopt-60.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-60.c"
# 9 "./strlenopt-60.c"
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
# 10 "./strlenopt-60.c" 2

void foo (char *);

size_t
f1 (void)
{
  char a[10] = "0123456789";
  return strnlen (a, 10);
}

size_t
f2 (void)
{
  char a[10] = "0123456789";
  return strnlen (a, 5);
}

size_t
f3 (void)
{
  char a[10] = "0123456789";
  return strnlen (a, 0);
}

size_t
f4 (void)
{
  char a[20];
  foo (a);
  memcpy (a, "0123456789", 10);
  return strnlen (a, 10);
}

size_t
f5 (void)
{
  char a[20];
  foo (a);
  memcpy (a, "0123456789", 10);
  return strnlen (a, 14);
}

size_t
f6 (void)
{
  char a[20];
  foo (a);
  return strnlen (a, 0);
}

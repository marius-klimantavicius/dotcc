//type: fp
//options: 
# 0 "./strlenopt-48.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-48.c"







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
# 9 "./strlenopt-48.c" 2

void f (void)
{
  extern char a[2][1];
  int n = strlen (a[1]);
  if (n >= sizeof a)
    abort();
}

void g (void)
{
  extern char b[3][2][1];
  int n = strlen (b[2][1]);
  if (n >= sizeof b)
    abort();
}

void h (void)
{
  extern char c[4][3][2][1];
  int n = strlen (c[3][2][1]);
  if (n >= sizeof c)
    abort();
}

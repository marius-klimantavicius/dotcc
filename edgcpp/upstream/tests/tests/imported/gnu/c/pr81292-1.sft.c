//type: rp
//options: 
# 0 "./pr81292-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr81292-1.c"



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
# 5 "./pr81292-1.c" 2

char a[10];

int __attribute__ ((noinline, noclone))
f1 (int n)
{
  a[0] = '1';
  a[1] = '2';
  return strlen (a + 1) < n ? strlen (a) : 100;
}

int __attribute__ ((noinline, noclone))
f2 (char *a, int n)
{
  a[0] = '1';
  a[1] = '2';
  return strlen (a + 1) < n ? strlen (a) : 100;
}

int
main (void)
{
  char b[10];
  strcpy (a + 2, "345");
  strcpy (b + 2, "34567");
  if (f1 (100) != 5 || f2 (b, 100) != 7)
    __builtin_abort ();
  return 0;
}

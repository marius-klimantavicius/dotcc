//type: rp
//options: 
# 0 "./pr81292-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr81292-2.c"



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
# 5 "./pr81292-2.c" 2

char a[] = { 0, 'a', 0, 'b', 'c', 0, 'd', 'e', 'f', 0 };

int __attribute__ ((noinline, noclone))
f1 (void)
{
  a[0] = '1';
  a[strlen (a)] = '2';
  a[strlen (a)] = '3';
  return strlen (a);
}

int __attribute__ ((noinline, noclone))
f2 (char *a)
{
  a[0] = '1';
  a[strlen (a)] = '2';
  a[strlen (a)] = '3';
  return strlen (a);
}

int
main (void)
{
  char b[] = { 0, 0, 'a', 'b', 0, 0 };
  if (f1 () != 9 || f2 (b) != 5)
    __builtin_abort ();
  return 0;
}

//type: rp
//options: 
# 0 "./strlenopt-28.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-28.c"



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
# 5 "./strlenopt-28.c" 2

volatile int v;

size_t
f1 (void)
{
  char a[30];
  v += 1;
  memcpy (a, "1234567", 8);
  memcpy (a + 7, "89abcdefg", 10);
  memcpy (a + 16, "h", 2);
  return strlen (a);
}

size_t
f2 (void)
{
  char a[30];
  v += 2;
  strcpy (a, "1234567");
  strcpy (a + 7, "89abcdefg");
  strcpy (a + 16, "h");
  return strlen (a);
}

size_t
f3 (char *a)
{
  v += 3;
  memcpy (a, "1234567", 8);
  memcpy (a + 7, "89abcdefg", 10);
  memcpy (a + 16, "h", 2);
  return strlen (a);
}

size_t
f4 (char *a)
{
  v += 4;
  strcpy (a, "1234567");
  strcpy (a + 7, "89abcdefg");
  strcpy (a + 16, "h");
  return strlen (a);
}

int
main ()
{
  char a[30];
  if (f1 () != 17 || f2 () != 17 || f3 (a) != 17 || f4 (a) != 17)
    abort ();
  return 0;
}

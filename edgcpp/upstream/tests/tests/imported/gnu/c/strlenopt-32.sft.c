//type: rp
//options: 
# 0 "./strlenopt-32.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-32.c"



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
# 5 "./strlenopt-32.c" 2

char temp[30];
volatile int v;

size_t __attribute__ ((noinline, noclone))
f1 (void)
{
  char a[30];
  v += 1;
  memcpy (a, "1234567", 7);
  memcpy (a + 7, "89abcdefg", 9);
  memcpy (a + 16, "h", 2);
  return strlen (a);
}

size_t __attribute__ ((noinline, noclone))
f2 (char *a)
{
  v += 2;
  memcpy (a, "1234567", 7);
  memcpy (a + 7, "89abcdefg", 9);
  memcpy (a + 16, "h", 2);
  return strlen (a);
}

size_t __attribute__ ((noinline, noclone))
f3 (void)
{
  char a[30];
  v += 3;
  a[0] = '1';
  memcpy (a + 1, "2345678", 8);
  return strlen (a);
}

size_t __attribute__ ((noinline, noclone))
f4 (char *a)
{
  v += 4;
  a[0] = '1';
  memcpy (a + 1, "2345678", 8);
  return strlen (a);
}

size_t __attribute__ ((noinline, noclone))
f5 (void)
{
  char a[30];
  v += 5;
  a[0] = '1';
  a[1] = '2';
  a[2] = '3';
  memcpy (a + 3, "456", 3);
  a[6] = '7';
  a[7] = 0;
  return strlen (a);
}

size_t __attribute__ ((noinline, noclone))
f6 (char *a)
{
  v += 6;
  a[0] = '1';
  a[1] = '2';
  a[2] = '3';
  memcpy (a + 3, "456", 3);
  a[6] = '7';
  a[7] = 0;
  return strlen (a);
}

size_t __attribute__ ((noinline, noclone))
f7 (void)
{
  char a[30];
  v += 7;
  strcpy (a, "abcde");
  int len1 = strlen (a);
  a[2] = '_';
  int len2 = strlen (a);
  return len1 + len2;
}

size_t __attribute__ ((noinline, noclone))
f8 (char *a)
{
  v += 8;
  strcpy (a, "abcde");
  int len1 = strlen (a);
  a[2] = '_';
  int len2 = strlen (a);
  return len1 + len2;
}

size_t __attribute__ ((noinline, noclone))
f9 (char b)
{
  char a[30];
  v += 9;
  strcpy (a, "foo.bar");
  a[4] = b;
  a[3] = 0;
  return strlen (a);
}

size_t __attribute__ ((noinline, noclone))
f10 (char *a, char b)
{
  v += 10;
  strcpy (a, "foo.bar");
  a[4] = b;
  a[3] = 0;
  return strlen (a);
}

size_t __attribute__ ((noinline, noclone))
f11 (void)
{
  char a[30];
  v += 11;
  strcpy (temp, "123456");
  memcpy (a, temp, 7);
  return strlen (a);
}

size_t __attribute__ ((noinline, noclone))
f12 (char *a)
{
  v += 12;
  strcpy (temp, "123456");
  memcpy (a, temp, 7);
  return strlen (a);
}

size_t __attribute__ ((noinline, noclone))
f13 (void)
{
  char a[30];
  v += 13;
  strcpy (temp, "1234567");
  memcpy (a, temp, 7);
  a[7] = 0;
  return strlen (a);
}

size_t __attribute__ ((noinline, noclone))
f14 (char *a)
{
  v += 14;
  strcpy (temp, "1234567");
  memcpy (a, temp, 7);
  a[7] = 0;
  return strlen (a);
}

size_t __attribute__ ((noinline, noclone))
f15 (void)
{
  char a[30];
  v += 15;
  strcpy (temp, "12345679");
  memcpy (a, temp, 7);
  a[7] = 0;
  return strlen (a);
}

size_t __attribute__ ((noinline, noclone))
f16 (char *a)
{
  v += 16;
  strcpy (temp, "123456789");
  memcpy (a, temp, 7);
  a[7] = 0;
  return strlen (a);
}

int
main ()
{
  char a[30];
  if (f1 () != 17 || f2 (a) != 17 || f3 () != 8 || f4 (a) != 8
      || f5 () != 7 || f6 (a) != 7 || f7 () != 10 || f8 (a) != 10
      || f9 ('_') != 3 || f10 (a, '_') != 3 || f11 () != 6 || f12 (a) != 6
      || f13 () != 7 || f14 (a) != 7 || f15 () != 7 || f16 (a) != 7)
    abort ();
  return 0;
}

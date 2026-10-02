//type: fp
//options: 
# 0 "./strlenopt-56.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-56.c"







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
# 9 "./strlenopt-56.c" 2

const char a[] = { 'a', 129, 0 };
const signed char b[] = { 'b', 130, 0 };
const unsigned char c[] = { 'c', 131, 0 };

const char s[] = "a\201";
const signed char ss[] = "b\202";
const unsigned char us[] = "c\203";




void test_values (void)
{
  ((a[0] == a[1]) ? (void)0 : __builtin_abort ());
  ((a[1] == 'a') ? (void)0 : __builtin_abort ());

  ((b[0] == b[1]) ? (void)0 : __builtin_abort ());
  ((b[1] == (signed char)'b') ? (void)0 : __builtin_abort ());

  ((c[0] == c[1]) ? (void)0 : __builtin_abort ());
  ((c[1] == (unsigned char)'c') ? (void)0 : __builtin_abort ());
}

void test_lengths (void)
{
  ((2 == strlen (a)) ? (void)0 : __builtin_abort ());
  ((2 == strlen ((const char*)b)) ? (void)0 : __builtin_abort ());
  ((2 == strlen ((const char*)c)) ? (void)0 : __builtin_abort ());
}

void test_contents (void)
{
  ((0 == strcmp (a, s)) ? (void)0 : __builtin_abort ());
  ((0 == strcmp ((const char*)b, (const char*)ss)) ? (void)0 : __builtin_abort ());
  ((0 == strcmp ((const char*)c, (const char*)us)) ? (void)0 : __builtin_abort ());
}

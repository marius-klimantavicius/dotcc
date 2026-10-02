//type: rp
//options: 
# 0 "./strlenopt-17g.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-17g.c"





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
# 7 "./strlenopt-17g.c" 2

__attribute__((noinline, noclone)) int
foo (const char *p)
{
  static int c;
  const char *q[] = { "123498765abcde", "123498765..", "129abcde", "129abcde" };
  if (strcmp (p, q[c]) != 0)
    abort ();
  return c++;
}

__attribute__((noinline, noclone)) void
bar (const char *p, const char *q)
{
  size_t l;

  char *a = __builtin_alloca (strlen (p) + 50);

  strcpy (a, p);

  strcat (a, q);

  l = strlen (a);

  strcat (a, "abcde");
  if (!foo (a))

    strcpy (a + l, "..");
  foo (a);
}

int
main ()
{
  const char *volatile s1 = "1234";
  const char *volatile s2 = "98765";
  const char *volatile s3 = "12";
  const char *volatile s4 = "9";
  bar (s1, s2);
  bar (s3, s4);
  return 0;
}

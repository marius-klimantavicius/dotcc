//type: rp
//options: 
# 0 "./strlenopt-20.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-20.c"



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
# 5 "./strlenopt-20.c" 2

__attribute__((noinline, noclone)) const char *
fn1 (int x, int y)
{
  const char *p;
  switch (x)
    {
    case 0:
      p = "abcd";

      asm volatile ("" : : : "memory");
      break;
    case 1:
      p = "efgh";
      break;
    case 2:
      p = "ijkl";
      break;
    default:
      p = "mnop";
      break;
    }
  if (y)

    return strchr (p, '\0');
  else

    return p + strlen (p + 1);
}

__attribute__((noinline, noclone)) size_t
fn2 (char *p, char *q)
{
  size_t l;

  strcpy (p, "abc");
  p[3] = 'd';
  l = strlen (p);
  strcpy (q, p);
  return l;
}

__attribute__((noinline, noclone)) char *
fn3 (char *p)
{
  char *c;


  strcpy (p, "abc");
  p[3] = 'd';
  c = strchr (p, '\0');
  strcat (p, "efgh");
  return c;
}

int
main ()
{
  int i;
  char buf[64], buf2[64];
  for (i = 0; i < 5; i++)
    {
      const char *p = "abcdefghijklmnop" + (i < 3 ? i : 3) * 4;
      const char *q;
      q = fn1 (i, 1);
      if (memcmp (q - 4, p, 4) != 0 || q[0] != '\0')
 abort ();
      q = fn1 (i, 0);
      if (memcmp (q - 3, p, 4) != 0 || q[1] != '\0')
 abort ();
    }
  memset (buf, '\0', sizeof buf);
  memset (buf + 4, 'z', 2);
  if (fn2 (buf, buf2) != 6
      || memcmp (buf, "abcdzz", 7) != 0
      || memcmp (buf2, "abcdzz", 7) != 0)
    abort ();
  memset (buf, '\0', sizeof buf);
  memset (buf + 4, 'z', 2);
  if (fn3 (buf) != buf + 6 || memcmp (buf, "abcdzzefgh", 11) != 0)
    abort ();
  return 0;
}

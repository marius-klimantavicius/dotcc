//type: rp
//options: 
# 0 "./strlenopt-19.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-19.c"



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
# 5 "./strlenopt-19.c" 2

__attribute__((noinline, noclone)) char *
fn1 (int x, int y, int z)
{
  static char buf[40];
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
    {
      strcpy (buf, p);
      if (z)
 strcat (buf, "ABCDEFG");
      else
 strcat (buf, "HIJKLMN");
    }
  else
    {
      strcpy (buf, p + 1);
      if (z)
 strcat (buf, "OPQ");
      else
 strcat (buf, "RST");
    }
  return buf;
}

int
main ()
{
  int i;
  for (i = 0; i < 5; i++)
    {
      const char *p = "abcdefghijklmnop" + (i < 3 ? i : 3) * 4;
      const char *q;
      fn1 (i ? 0 : 1, 1, 1);
      q = fn1 (i, 0, 0);
      if (memcmp (q, p + 1, 3) != 0 || memcmp (q + 3, "RST", 4) != 0)
 abort ();
      fn1 (i ? 0 : 1, 0, 1);
      q = fn1 (i, 1, 0);
      if (memcmp (q, p, 4) != 0 || memcmp (q + 4, "HIJKLMN", 8) != 0)
 abort ();
      fn1 (i ? 0 : 1, 1, 0);
      q = fn1 (i, 0, 1);
      if (memcmp (q, p + 1, 3) != 0 || memcmp (q + 3, "OPQ", 4) != 0)
 abort ();
      fn1 (i ? 0 : 1, 0, 0);
      q = fn1 (i, 1, 1);
      if (memcmp (q, p, 4) != 0 || memcmp (q + 4, "ABCDEFG", 8) != 0)
 abort ();
    }
  return 0;
}

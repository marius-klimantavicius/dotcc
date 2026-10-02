//type: rp
//options: 
# 0 "./strlenopt-13.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-13.c"



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
# 5 "./strlenopt-13.c" 2

__attribute__((noinline, noclone)) void
fn1 (char *p, const char *y, const char *z, size_t *lp)
{
  char *q, *r, *s;
  char buf1[64], buf2[64];
  size_t l[8];



  l[0] = strlen (y);
  l[1] = strlen (z);
  strcpy (buf1, y);
  strcpy (buf2, z);
  strcpy (p, "abcde");
  q = strchr (p, '\0');
  strcpy (q, "efghi");
  r = strchr (q, '\0');
  strcpy (r, buf1);
  l[2] = strlen (p);
  l[3] = strlen (q);
  l[4] = strlen (r);
  strcpy (r, buf2);



  l[5] = strlen (p);
  l[6] = strlen (q);

  l[7] = strlen (r);
  memcpy (lp, l, sizeof l);
}

int
main ()
{
  char buf[64];
  size_t l[8];
  const char *volatile y = "ABCDEFG";
  const char *volatile z = "HIJK";
  memset (buf, '\0', sizeof buf);
  fn1 (buf, y, z, l);
  if (memcmp (buf, "abcdeefghiHIJK", 15) != 0)
    abort ();
  if (l[0] != 7 || l[1] != 4)
    abort ();
  if (l[2] != 17 || l[3] != 12 || l[4] != 7)
    abort ();
  if (l[5] != 14 || l[6] != 9 || l[7] != 4)
    abort ();
  return 0;
}

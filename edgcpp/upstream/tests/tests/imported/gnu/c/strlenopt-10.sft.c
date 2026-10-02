//type: rp
//options: 
# 0 "./strlenopt-10.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-10.c"



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
# 5 "./strlenopt-10.c" 2

__attribute__((noinline, noclone)) size_t
fn1 (char *p)
{
  char *q;


  strcpy (p, "z");
  q = strchr (p, '\0');
  *q = 32;

  return strlen (p);
}

__attribute__((noinline, noclone)) void
fn2 (char *p, const char *z, size_t *lp)
{
  char *q, *r;
  char buf[64];
  size_t l[10];



  l[0] = strlen (z);
  strcpy (buf, z);
  strcpy (p, "abcde");
  q = strchr (p, '\0');
  strcpy (q, "efghi");
  r = strchr (q, '\0');
  strcpy (r, "jkl");
  l[1] = strlen (p);
  l[2] = strlen (q);
  l[3] = strlen (r);
  strcpy (r, buf);
  l[4] = strlen (p);
  l[5] = strlen (q);
  l[6] = strlen (r);
  strcpy (r, "mnopqr");
  l[7] = strlen (p);
  l[8] = strlen (q);
  l[9] = strlen (r);
  memcpy (lp, l, sizeof l);
}

int
main ()
{
  char buf[64];
  size_t l[10];
  const char *volatile z = "ABCDEFG";
  memset (buf, '\0', sizeof buf);
  if (fn1 (buf) != 2 || buf[0] != 'z' || buf[1] != 32 || buf[2] != '\0')
    abort ();
  fn2 (buf, z, l);
  if (memcmp (buf, "abcdeefghimnopqr", 17) != 0)
    abort ();
  if (l[0] != 7)
    abort ();
  if (l[1] != 13 || l[2] != 8 || l[3] != 3)
    abort ();
  if (l[4] != 17 || l[5] != 12 || l[6] != 7)
    abort ();
  if (l[7] != 16 || l[8] != 11 || l[9] != 6)
    abort ();
  return 0;
}

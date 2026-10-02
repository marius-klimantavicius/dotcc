//type: rp
//options: 
# 0 "./strlenopt-6.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-6.c"



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
# 5 "./strlenopt-6.c" 2

__attribute__((noinline, noclone)) char *
foo (char *x)
{
# 18 "./strlenopt-6.c"
  char b[64];
  char *q = malloc (strlen (x) + 64);
  char *p = strcpy (b, x);

  char *r;
  if (q == ((void *) 0)) return ((void *) 0);


  *q = '\0';

  strcat (q, p);
  strcat (q, "/");


  r = strchr (q, '\0');


  *r = '\0';

  strcat (q, "abcde");
  return q;
}

__attribute__((noinline, noclone)) char *
bar (char *p)
{
  char buf[26];
  char *r;
  if (strlen (p) + 9 > 26)
    return ((void *) 0);
  *buf = '\0';
  strcat (buf, p);
  strcat (buf, "/");
  r = strchr (buf, '\0');
  *r = '\0';
  strcat (buf, "abcde");
  return strdup (buf);
}

int
main ()
{
  char *volatile p = "string1";
  char *volatile r = "string2";
  char *q = foo (p);
  if (q != ((void *) 0))
    {
      if (strcmp (q, "string1/abcde"))
 abort ();
      memset (q, '\0', 14);
      free (q);
    }
  q = bar (p);
  if (q != ((void *) 0))
    {
      if (strcmp (q, "string1/abcde"))
 abort ();
      free (q);
    }
  return 0;
}

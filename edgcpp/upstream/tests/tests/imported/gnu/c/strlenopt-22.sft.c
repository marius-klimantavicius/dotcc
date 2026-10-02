//type: rp
//options: 
# 0 "./strlenopt-22.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-22.c"



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
# 5 "./strlenopt-22.c" 2

__attribute__((noinline, noclone)) size_t
bar (char *p, char *q)
{
  size_t l1, l2, l3;
  char *r = strchr (p, '\0');
  strcpy (r, "abcde");
  char *s = strchr (r, '\0');
  strcpy (s, q);
  l1 = strlen (p);
  l2 = strlen (r);
  l3 = strlen (s);
  return l1 + l2 + l3;
}

int
main ()
{
  char buf[16] = "01234";

  if (bar (buf, "56789") != 30)
    abort ();

  if (memcmp (buf, "01234abcde56789", 16) != 0)
    abort ();

  return 0;
}

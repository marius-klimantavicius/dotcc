//type: fp
//options: 
# 0 "./strlenopt-38.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-38.c"





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
# 7 "./strlenopt-38.c" 2

void
foo (void)
{
  char a[5] = "012";
  strcpy (a, "");
  if (strlen (a) != 0)
    abort ();
}

void
bar (void)
{
  char a[5] = "012";
  char b[7] = "";
  strcpy (a, b);
  if (strlen (a) != 0)
    abort ();
}

struct S { char a[4]; char b[5]; char c[7]; };

void
baz (void)
{
  struct S s;
  strcpy (s.b, "012");
  strcpy (s.c, "");
  strcpy (s.b, s.c);
  if (s.b[0] != 0)
    abort ();
}

void
boo (void)
{
  struct S s;
  strcpy (s.b, "012");
  strcpy (s.c, "");
  strcpy (s.b, s.c);
  if (strlen (s.b) != 0)
    abort ();
}

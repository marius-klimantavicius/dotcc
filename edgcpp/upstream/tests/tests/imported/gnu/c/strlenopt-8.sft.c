//type: rp
//options: 
# 0 "./strlenopt-8.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-8.c"



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
# 5 "./strlenopt-8.c" 2



__attribute__((noinline, noclone)) char *
foo (int r)
{
  char buf[10] = "";
  strcat (buf, r ? "r" : "w");
  strcat (buf, "b");
  return strdup (buf);
}

__attribute__((noinline, noclone)) char *
bar (int r)
{
  char buf[10] = {};
  strcat (buf, r ? "r" : "w");
  strcat (buf, "b");
  return strdup (buf);
}

int
main ()
{
  char *q = foo (1);
  if (q != ((void *) 0))
    {
      if (strcmp (q, "rb"))
 abort ();
      free (q);
    }
  q = bar (0);
  if (q != ((void *) 0))
    {
      if (strcmp (q, "wb"))
 abort ();
      free (q);
    }
  return 0;
}

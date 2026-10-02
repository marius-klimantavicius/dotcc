//type: rp
//options: 
# 0 "./strlenopt-31g.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-31g.c"




# 1 "./strlenopt-31.c" 1



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
# 5 "./strlenopt-31.c" 2

__attribute__((noinline, noclone)) int
bar (char *p1, const char *q)
{
  strcpy (p1, "abcde");
  char *p2 = strchr (p1, '\0');
  strcpy (p2, q);
  char *p3 = strchr (p2, '\0');
  memcpy (p3, "x", 2);
  return strlen (p1);
}

int
main (void)
{
  char buffer[10];
  int res = bar (buffer, "foo");
  if (strcmp (buffer, "abcdefoox") != 0 || res != 9)
    abort ();
  return 0;
}
# 6 "./strlenopt-31g.c" 2

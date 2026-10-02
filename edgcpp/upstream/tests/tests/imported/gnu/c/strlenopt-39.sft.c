//type: fp
//options: 
# 0 "./strlenopt-39.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-39.c"




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
# 6 "./strlenopt-39.c" 2



const char str[] = "1234567";

char dst[10];

void copy_from_global_str (void)
{
  strcpy (dst, str);

  if (strlen (dst) != sizeof str - 1)
    abort ();
}

void copy_from_local_str (void)
{
  const char s[] = "1234567";

  strcpy (dst, s);

  if (strlen (dst) != sizeof s - 1)
    abort ();
}

void copy_from_local_memstr (void)
{
  struct {
    char s[sizeof "1234567"];
  } x = { "1234567" };

  strcpy (dst, x.s);

  if (strlen (dst) != sizeof x.s - 1)
    abort ();
}

void copy_to_local_str (void)
{
  char d[sizeof "1234567"];

  strcpy (d, str);

  if (strlen (d) != sizeof str - 1)
    abort ();
}

void copy_to_local_memstr (void)
{
  struct {
    char d[sizeof "1234567"];
  } x;

  strcpy (x.d, str);

  if (strlen (x.d) != sizeof str- 1)
    abort ();
}

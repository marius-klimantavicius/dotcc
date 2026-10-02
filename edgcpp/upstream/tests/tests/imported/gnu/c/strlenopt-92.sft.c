//type: rp
//options: 
# 0 "./strlenopt-92.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-92.c"




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
# 6 "./strlenopt-92.c" 2

__attribute__((noipa)) int
copy (char *x, int y)
{
  if (y == 0)
    strcpy (x, "abcd");
  return y;
}

__attribute__((noipa)) char *
alloc_2_copy_compare (int x)
{
  char *p;
  if (x)
    p = malloc (4);
  else
    p = calloc (16, 1);

  char *q = p + 2;
  if (copy (q, x))
    return p;

  if (strcmp (q, "abcd") != 0)
    abort ();

  return p;
}

char a5[5], a6[6], a7[7];

__attribute__((noipa)) char *
decl_3_copy_compare (int x)
{
  char *p = x < 0 ? a5 : 0 < x ? a6 : a7;
  char *q = p + 1;
  if (copy (q, x))
    return p;

  if (strcmp (q, "abcd") != 0)
    abort ();

  return p;
}

int main ()
{
  free (alloc_2_copy_compare (0));
  free (alloc_2_copy_compare (1));

  decl_3_copy_compare (-1);
  decl_3_copy_compare (0);
  decl_3_copy_compare (1);
}

//type: fp
//options: 
# 0 "./strlenopt-49.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-49.c"





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
# 7 "./strlenopt-49.c" 2

const char a1[1] = "\0";
const char a2[2] = "1\0";
const char a3[3] = "12\0";
const char a8[8] = "1234567\0";
const char a9[9] = "12345678\0";

const char ax[9] = "12345678\0\0\0\0";
const char ay[9] = "\00012345678\0\0\0\0";


int len1 (void)
{
  size_t len0 = strlen (a1);
  return len0;
}

int len (void)
{
  size_t len = strlen (a2) + strlen (a3) + strlen (a8) + strlen (a9);
  return len;
}

int lenx (void)
{
  size_t lenx = strlen (ax);
  return lenx;
}

int leny (void)
{
  size_t leny = strlen (ay);
  return leny;
}

int cmp88 (void)
{
  int cmp88 = memcmp (a8, "1234567\0", sizeof a8);
  return cmp88;
}

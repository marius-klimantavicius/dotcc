//type: fp
//options: 
# 0 "./strlenopt-95.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-95.c"




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
# 6 "./strlenopt-95.c" 2



typedef __attribute__ ((vector_size (1))) char VC1;
typedef __attribute__ ((vector_size (2))) char VC2;
typedef __attribute__ ((vector_size (4))) char VC4;
typedef __attribute__ ((vector_size (8))) char VC8;
typedef __attribute__ ((vector_size (16))) char VC16;

extern char a[];



void test_fold (int i)
{
  *(VC4*)a = (VC4){ };
  ((strlen (a) == 0) ? (void)0 : abort ());
  ((!a[1] && !a[2] && !a[3]) ? (void)0 : abort ());

  *(VC4*)a = (VC4){ 0, 1 };
  ((strlen (a) == 0) ? (void)0 : abort ());
  ((a[1] == 1 && !a[2] && !a[3]) ? (void)0 : abort ());

  *(VC4*)a = (VC4){ 1 };
  ((strlen (a) == 1) ? (void)0 : abort ());
  ((!a[1] && !a[2] && !a[3]) ? (void)0 : abort ());

  *(VC4*)a = (VC4){ 1, 0, 3 };
  ((strlen (a) == 1) ? (void)0 : abort ());
  ((!a[1] && a[2] == 3 && !a[3]) ? (void)0 : abort ());

  *(VC4*)a = (VC4){ 1, 2 };
  ((strlen (a) == 2) ? (void)0 : abort ());
  ((!a[2] && !a[3]) ? (void)0 : abort ());

  *(VC4*)a = (VC4){ 1, 2, 0, 4 };
  ((strlen (a) == 2) ? (void)0 : abort ());
  ((!a[2] && a[3] == 4) ? (void)0 : abort ());

  *(VC4*)a = (VC4){ 1, 2, 3 };
  ((strlen (a) == 3) ? (void)0 : abort ());
  ((!a[3]) ? (void)0 : abort ());

  *(VC8*)a = (VC8){ 1, 2, 3, 0, 5 };
  ((strlen (a) == 3) ? (void)0 : abort ());

  *(VC8*)a = (VC8){ 1, 2, 3, 0, 5, 6 };
  ((strlen (a) == 3) ? (void)0 : abort ());

  *(VC8*)a = (VC8){ 1, 2, 3, 0, 5, 6, 7, 8 };
  ((strlen (a) == 3) ? (void)0 : abort ());
  ((strlen (a + 1) == 2) ? (void)0 : abort ());
  ((strlen (a + 2) == 1) ? (void)0 : abort ());
  ((strlen (a + 3) == 0) ? (void)0 : abort ());

  ((a[4] == 5 && a[5] == 6 && a[6] == 7 && a[7] == 8) ? (void)0 : abort ());
}

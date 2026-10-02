//type: fp
//options: 
# 0 "./strlenopt-83.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-83.c"





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
# 7 "./strlenopt-83.c" 2
char *p_p2, *p_p5, *p_p9, *p_p14;

unsigned n0, n1, n2, n3, n4;


static inline __attribute__ ((always_inline)) void
elim_strlen_of_consecutive_strcpy (char *p)
{
  p_p2 = p + 2;
  __builtin_strcpy (p_p2, "12");

  p_p5 = p_p2 + 3;
  __builtin_strcpy (p_p5, "124");

  p_p9 = p_p5 + 4;
  __builtin_strcpy (p_p9, "1245");

  p_p14 = p_p9 + 5;

  n0 = __builtin_strlen (p);
  n1 = __builtin_strlen (p_p2);
  n2 = __builtin_strlen (p_p5);
  n3 = __builtin_strlen (p_p9);




  if (n0 || n1 != 2 || n2 != 3 || n3 != 4)
    __builtin_abort ();
}


void elim_strlen_of_consecutive_strcpy_in_alloca (unsigned n)
{

  n = 14;

  char *p = __builtin_alloca (n);

  *p = '\0';

  elim_strlen_of_consecutive_strcpy (p);
}


void elim_strlen_of_consecutive_strcpy_in_vla (unsigned n)
{

  n = 14;

  char vla[n];

  *vla = '\0';

  elim_strlen_of_consecutive_strcpy (vla);
}

void elim_strlen_of_consecutive_strcpy_in_malloc (unsigned n)
{
  char *p = __builtin_malloc (n);

  *p = '\0';

  elim_strlen_of_consecutive_strcpy (p);
}


void elim_strlen_of_consecutive_strcpy_in_calloc (unsigned n)
{
  char *p = __builtin_calloc (n, 1);



  elim_strlen_of_consecutive_strcpy (p);
}

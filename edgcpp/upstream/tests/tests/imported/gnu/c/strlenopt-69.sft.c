//type: fp
//options: 
# 0 "./strlenopt-69.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-69.c"





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
# 7 "./strlenopt-69.c" 2
# 15 "./strlenopt-69.c"
void clobber (void*, ...);

struct S { char a4[4], c; };

extern char a4[4];
extern char b4[4];





void test_array_lit (void)
{
  ((strcmp (a4, "1234")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 28, "strcmp (a4, \"1234\")"), __builtin_abort ())); clobber (a4);
  ((strcmp (a4, "12345")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 29, "strcmp (a4, \"12345\")"), __builtin_abort ())); clobber (a4);
  ((strcmp (a4, "123456")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 30, "strcmp (a4, \"123456\")"), __builtin_abort ())); clobber (a4);
  ((strcmp ("1234", a4)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 31, "strcmp (\"1234\", a4)"), __builtin_abort ())); clobber (a4);
  ((strcmp ("12345", a4)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 32, "strcmp (\"12345\", a4)"), __builtin_abort ())); clobber (a4);
  ((strcmp ("123456", a4)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 33, "strcmp (\"123456\", a4)"), __builtin_abort ())); clobber (a4);
}

void test_memarray_lit (struct S *p)
{
# 48 "./strlenopt-69.c"
}



void test_empty_string (void)
{
  ((0 == strcmp ("", "")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 54, "0 == strcmp (\"\", \"\")"), __builtin_abort ()));

  *a4 = '\0';
  ((0 == strcmp (a4, "")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 57, "0 == strcmp (a4, \"\")"), __builtin_abort ()));
  ((0 == strcmp ("", a4)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 58, "0 == strcmp (\"\", a4)"), __builtin_abort ()));
  ((0 == strcmp (a4, a4)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 59, "0 == strcmp (a4, a4)"), __builtin_abort ()));

  char s[8] = "";
  ((0 == strcmp (a4, s)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 62, "0 == strcmp (a4, s)"), __builtin_abort ()));

  a4[1] = '\0';
  b4[1] = '\0';
  ((0 == strcmp (a4 + 1, b4 + 1)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 66, "0 == strcmp (a4 + 1, b4 + 1)"), __builtin_abort ()));

  a4[2] = '\0';
  b4[2] = '\0';
  ((0 == strcmp (&a4[2], &b4[2])) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 70, "0 == strcmp (&a4[2], &b4[2])"), __builtin_abort ()));
# 80 "./strlenopt-69.c"
}




void test_array_copy (void)
{
  char s[8];
  strcpy (s, "1234");
  ((strcmp (a4, s)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 89, "strcmp (a4, s)"), __builtin_abort ()));

  strcpy (s, "12345");
  ((strlen (s) == 5) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 92, "strlen (s) == 5"), __builtin_abort ()));
  ((strcmp (a4, s)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 93, "strcmp (a4, s)"), __builtin_abort ())); clobber (a4);

  strcpy (s, "123456");
  ((strcmp (a4, s)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 96, "strcmp (a4, s)"), __builtin_abort ())); clobber (a4);

  strcpy (s, "1234");
  ((strcmp (s, a4)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 99, "strcmp (s, a4)"), __builtin_abort ())); clobber (a4);

  strcpy (s, "12345");
  ((strcmp (s, a4)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 102, "strcmp (s, a4)"), __builtin_abort ())); clobber (a4);

  strcpy (s, "123456");
  ((strcmp (s, a4)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 105, "strcmp (s, a4)"), __builtin_abort ())); clobber (a4);
}


void test_array_bounded (void)
{
  ((strncmp (a4, "12345", 5)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 111, "strncmp (a4, \"12345\", 5)"), __builtin_abort ())); clobber (a4);
  ((strncmp ("54321", a4, 5)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 112, "strncmp (\"54321\", a4, 5)"), __builtin_abort ())); clobber (a4);

  ((strncmp (a4, "123456", 5)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 114, "strncmp (a4, \"123456\", 5)"), __builtin_abort ())); clobber (a4);
  ((strncmp ("654321", a4, 5)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 115, "strncmp (\"654321\", a4, 5)"), __builtin_abort ())); clobber (a4);
}

void test_array_copy_bounded (void)
{
  char s[8];
  strcpy (s, "12345");
  ((strncmp (a4, s, 5)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 122, "strncmp (a4, s, 5)"), __builtin_abort ())); clobber (a4);
  strcpy (s, "54321");
  ((strncmp (s, a4, 5)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 124, "strncmp (s, a4, 5)"), __builtin_abort ())); clobber (a4);

  strcpy (s, "123456");
  ((strncmp (a4, s, 5)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 127, "strncmp (a4, s, 5)"), __builtin_abort ())); clobber (a4);
  strcpy (s, "654321");
  ((strncmp (s, a4, 5)) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 129, "strncmp (s, a4, 5)"), __builtin_abort ())); clobber (a4);
}

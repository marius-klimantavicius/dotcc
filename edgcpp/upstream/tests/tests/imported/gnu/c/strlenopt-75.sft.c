//type: rp
//options: 
# 0 "./strlenopt-75.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-75.c"





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
# 7 "./strlenopt-75.c" 2



int i = 0;

const char s[] = "1234567";

char a[32];






__attribute__ ((noclone, noinline, noipa)) void test_memcpy_same_length ()
{
  memcpy (a, "123456789a", 11);
  memcpy (a + 6, i ? "78\0" : "789\0", 4);
  if (strlen (a) != 9)
    abort ();
}



__attribute__ ((noclone, noinline, noipa)) void test_strcpy_strcat_same_length ()
{
  strcpy (a, "12345678");
  strcat (a, "9a");
  memcpy (a + 6, i ? "78\0" : "789\0", 4);
  if (strlen (a) != 9)
    abort ();
}




__attribute__ ((noclone, noinline, noipa)) void test_assign_same_length ()
{
  memcpy (a, s, 8);
  memcpy (a + 5, i ? "67\0" : "678\0", 4);
  if (strlen (a) != 8)
    abort ();
}




__attribute__ ((noclone, noinline, noipa)) void test_memcpy_lengthen ()
{
  memcpy (a, "123456789a", 11);
  memcpy (a + 8, i ? "9a\0" : "9ab\0", 4);
  if (strlen (a) != 11)
    abort ();
}

__attribute__ ((noclone, noinline, noipa)) void test_strcpy_strcat_lengthen ()
{
  strcpy (a, "12345678");
  strcat (a, "9a");
  memcpy (a + 8, i ? "9a\0" : "9ab\0", 4);
  if (strlen (a) != 11)
    abort ();
}

__attribute__ ((noclone, noinline, noipa)) void test_assign_lengthen ()
{
  memcpy (a, s, 8);
  memcpy (a + 6, i ? "78\0" : "789\0", 4);
  if (strlen (a) != 9)
    abort ();
}

__attribute__ ((noclone, noinline, noipa)) void test_memcpy_shorten ()
{
  memcpy (a, "123456789a", 11);
  memcpy (a + 6, i ? "789\0" : "78\0", 4);
  if (strlen (a) != 8)
    abort ();
}

__attribute__ ((noclone, noinline, noipa)) void test_strcpy_strcat_shorten ()
{
  strcpy (a, "12345678");
  strcat (a, "9a");
  memcpy (a + 6, i ? "789\0" : "78\0", 4);
  if (strlen (a) != 8)
    abort ();
}

__attribute__ ((noclone, noinline, noipa)) void test_assign_shorten ()
{
  memcpy (a, s, 8);
  memcpy (a + 6, i ? "789\0" : "78\0", 4);
  if (strlen (a) != 8)
    abort ();
}


int main (void)
{
  test_memcpy_same_length ();
  test_strcpy_strcat_same_length ();
  test_assign_same_length ();

  test_memcpy_lengthen ();
  test_strcpy_strcat_lengthen ();
  test_assign_lengthen ();

  test_memcpy_shorten ();
  test_strcpy_strcat_shorten ();
  test_assign_shorten ();
}

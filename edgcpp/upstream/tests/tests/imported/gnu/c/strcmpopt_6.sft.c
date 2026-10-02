//type: rp
//options: 
# 0 "./strcmpopt_6.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strcmpopt_6.c"





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
# 7 "./strcmpopt_6.c" 2
# 15 "./strcmpopt_6.c"
__attribute__ ((noclone, noinline)) int
test_strlen_gt2_strcmp_abcd (const char *s)
{
  if (strlen (s) < 3)
    return -1;

  return strcmp (s, "abcd") == 0;
}

__attribute__ ((noclone, noinline)) int
test_strlen_lt6_strcmp_abcd (const char *s)
{
  if (strlen (s) > 5)
    return -1;

  return strcmp (s, "abcd") == 0;
}

__attribute__ ((noclone, noinline)) int
test_strcpy_strcmp_abc (const char *s)
{
  char a[5];
  strcpy (a, s);
  return strcmp (a, "abc") == 0;
}

__attribute__ ((noclone, noinline)) int
test_strcpy_abc_strcmp (const char *s)
{
  char a[4], b[6];
  strcpy (a, "abc");
  strcpy (b, s);
  return strcmp (a, b) == 0;
}



char ga4[4], gb4[4];

__attribute__ ((noclone, noinline)) int
test_store_0_nulterm_strcmp_same_size_arrays (void)
{
  ga4[0] = gb4[0] = 'x';
  ga4[3] = gb4[3] = '\0';
  return strcmp (ga4, gb4) == 0;
}

__attribute__ ((noclone, noinline)) int
test_store_0_nulterm_strncmp_bound_2_same_size_arrays (void)
{
  ga4[0] = gb4[0] = 'x';
  ga4[3] = gb4[3] = '\0';
  return strncmp (ga4, gb4, 2) == 0;
}

__attribute__ ((noclone, noinline)) int
test_store_0_nulterm_strncmp_bound_equal_same_size_arrays (void)
{
  ga4[0] = gb4[0] = 'x';
  ga4[3] = gb4[3] = '\0';
  return strncmp (ga4, gb4, 4) == 0;
}




__attribute__ ((noclone, noinline)) int
test_nulterm_strcmp_same_size_arrays (void)
{
  ga4[3] = gb4[3] = '\0';
  return strcmp (ga4, gb4) == 0;
}



char gc5[5];

__attribute__ ((noclone, noinline)) int
test_store_0_nulterm_strcmp_arrays (void)
{
  ga4[0] = gc5[0] = 'x';
  ga4[3] = gc5[4] = '\0';
  return strcmp (ga4, gc5) == 0;
}




__attribute__ ((noclone, noinline)) int
test_nulterm_strcmp_arrays (void)
{
  ga4[3] = gc5[4] = '\0';
  return strcmp (ga4, gc5) == 0;
}


__attribute__ ((noclone, noinline)) int
test_strcpy_strncmp_abcd (const char *s)
{
  char a[6];
  strcpy (a, s);
  return strcmp (a, "abcd") == 0;
}

__attribute__ ((noclone, noinline)) int
test_strcpy_abcd_strncmp_3 (const char *s)
{
  char a[6], b[8];
  strcpy (a, "abcd");
  strcpy (b, s);
  return strncmp (a, b, 3) == 0;
}

__attribute__ ((noclone, noinline)) int
test_strcpy_abcd_strncmp_4 (const char *s)
{
  char a[6], b[8];
  strcpy (a, "abcd");
  strcpy (b, s);
  return strncmp (a, b, 4) == 0;
}


int main (void)
{
  test_strlen_gt2_strcmp_abcd ("abcd");
  test_strlen_lt6_strcmp_abcd ("abcd");

  ((0 == test_strcpy_strcmp_abc ("ab")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 143, "0 == test_strcpy_strcmp_abc (\"ab\")"), __builtin_abort ()));
  ((0 != test_strcpy_strcmp_abc ("abc")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 144, "0 != test_strcpy_strcmp_abc (\"abc\")"), __builtin_abort ()));
  ((0 == test_strcpy_strcmp_abc ("abcd")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 145, "0 == test_strcpy_strcmp_abc (\"abcd\")"), __builtin_abort ()));

  ((0 == test_strcpy_abc_strcmp ("ab")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 147, "0 == test_strcpy_abc_strcmp (\"ab\")"), __builtin_abort ()));
  ((0 != test_strcpy_abc_strcmp ("abc")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 148, "0 != test_strcpy_abc_strcmp (\"abc\")"), __builtin_abort ()));
  ((0 == test_strcpy_abc_strcmp ("abcd")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 149, "0 == test_strcpy_abc_strcmp (\"abcd\")"), __builtin_abort ()));

  strcpy (ga4, "abc"); strcpy (gb4, "abd");
  ((0 == test_store_0_nulterm_strcmp_same_size_arrays ()) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 152, "0 == test_store_0_nulterm_strcmp_same_size_arrays ()"), __builtin_abort ()));
  strcpy (ga4, "abd"); strcpy (gb4, "abc");
  ((0 == test_store_0_nulterm_strcmp_same_size_arrays ()) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 154, "0 == test_store_0_nulterm_strcmp_same_size_arrays ()"), __builtin_abort ()));
  strcpy (ga4, "abc"); strcpy (gb4, "abc");
  ((0 != test_store_0_nulterm_strcmp_same_size_arrays ()) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 156, "0 != test_store_0_nulterm_strcmp_same_size_arrays ()"), __builtin_abort ()));

  strcpy (ga4, "abc"); strcpy (gb4, "acd");
  ((0 == test_store_0_nulterm_strncmp_bound_2_same_size_arrays ()) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 159, "0 == test_store_0_nulterm_strncmp_bound_2_same_size_arrays ()"), __builtin_abort ()));
  strcpy (ga4, "acd"); strcpy (gb4, "abc");
  ((0 == test_store_0_nulterm_strncmp_bound_2_same_size_arrays ()) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 161, "0 == test_store_0_nulterm_strncmp_bound_2_same_size_arrays ()"), __builtin_abort ()));
  strcpy (ga4, "abc"); strcpy (gb4, "abc");
  ((0 != test_store_0_nulterm_strncmp_bound_2_same_size_arrays ()) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 163, "0 != test_store_0_nulterm_strncmp_bound_2_same_size_arrays ()"), __builtin_abort ()));

  strcpy (ga4, "abc"); strcpy (gb4, "abd");
  ((0 == test_store_0_nulterm_strncmp_bound_equal_same_size_arrays ()) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 166, "0 == test_store_0_nulterm_strncmp_bound_equal_same_size_arrays ()"), __builtin_abort ()));
  strcpy (ga4, "abd"); strcpy (gb4, "abc");
  ((0 == test_store_0_nulterm_strncmp_bound_equal_same_size_arrays ()) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 168, "0 == test_store_0_nulterm_strncmp_bound_equal_same_size_arrays ()"), __builtin_abort ()));
  strcpy (ga4, "abc"); strcpy (gb4, "abc");
  ((0 != test_store_0_nulterm_strncmp_bound_equal_same_size_arrays ()) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 170, "0 != test_store_0_nulterm_strncmp_bound_equal_same_size_arrays ()"), __builtin_abort ()));

  strcpy (ga4, "abc"); strcpy (gb4, "abd");
  ((0 == test_nulterm_strcmp_same_size_arrays ()) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 173, "0 == test_nulterm_strcmp_same_size_arrays ()"), __builtin_abort ()));
  strcpy (ga4, "abd"); strcpy (gb4, "abc");
  ((0 == test_nulterm_strcmp_same_size_arrays ()) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 175, "0 == test_nulterm_strcmp_same_size_arrays ()"), __builtin_abort ()));
  strcpy (ga4, "abc"); strcpy (gb4, "abc");
  ((0 != test_nulterm_strcmp_same_size_arrays ()) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 177, "0 != test_nulterm_strcmp_same_size_arrays ()"), __builtin_abort ()));

  strcpy (ga4, "abc"); strcpy (gc5, "abcd");
  ((0 == test_store_0_nulterm_strcmp_arrays ()) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 180, "0 == test_store_0_nulterm_strcmp_arrays ()"), __builtin_abort ()));
  strcpy (ga4, "abd"); strcpy (gc5, "abcd");
  ((0 == test_store_0_nulterm_strcmp_arrays ()) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 182, "0 == test_store_0_nulterm_strcmp_arrays ()"), __builtin_abort ()));
  strcpy (ga4, "abc"); strcpy (gc5, "abc");
  ((0 != test_store_0_nulterm_strcmp_arrays ()) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 184, "0 != test_store_0_nulterm_strcmp_arrays ()"), __builtin_abort ()));

  strcpy (ga4, "abc"); strcpy (gc5, "abcd");
  ((0 == test_nulterm_strcmp_arrays ()) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 187, "0 == test_nulterm_strcmp_arrays ()"), __builtin_abort ()));
  strcpy (ga4, "abd"); strcpy (gc5, "abc");
  ((0 == test_nulterm_strcmp_arrays ()) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 189, "0 == test_nulterm_strcmp_arrays ()"), __builtin_abort ()));
  strcpy (ga4, "abc"); strcpy (gc5, "abc");
  ((0 != test_nulterm_strcmp_arrays ()) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 191, "0 != test_nulterm_strcmp_arrays ()"), __builtin_abort ()));

  ((0 == test_strcpy_strncmp_abcd ("ab")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 193, "0 == test_strcpy_strncmp_abcd (\"ab\")"), __builtin_abort ()));
  ((0 == test_strcpy_strncmp_abcd ("abc")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 194, "0 == test_strcpy_strncmp_abcd (\"abc\")"), __builtin_abort ()));
  ((0 != test_strcpy_strncmp_abcd ("abcd")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 195, "0 != test_strcpy_strncmp_abcd (\"abcd\")"), __builtin_abort ()));
  ((0 == test_strcpy_strncmp_abcd ("abcde")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 196, "0 == test_strcpy_strncmp_abcd (\"abcde\")"), __builtin_abort ()));

  ((0 == test_strcpy_abcd_strncmp_3 ("ab")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 198, "0 == test_strcpy_abcd_strncmp_3 (\"ab\")"), __builtin_abort ()));
  ((0 != test_strcpy_abcd_strncmp_3 ("abc")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 199, "0 != test_strcpy_abcd_strncmp_3 (\"abc\")"), __builtin_abort ()));
  ((0 != test_strcpy_abcd_strncmp_3 ("abcd")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 200, "0 != test_strcpy_abcd_strncmp_3 (\"abcd\")"), __builtin_abort ()));
  ((0 != test_strcpy_abcd_strncmp_3 ("abcde")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 201, "0 != test_strcpy_abcd_strncmp_3 (\"abcde\")"), __builtin_abort ()));

  ((0 == test_strcpy_abcd_strncmp_4 ("ab")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 203, "0 == test_strcpy_abcd_strncmp_4 (\"ab\")"), __builtin_abort ()));
  ((0 == test_strcpy_abcd_strncmp_4 ("abc")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 204, "0 == test_strcpy_abcd_strncmp_4 (\"abc\")"), __builtin_abort ()));
  ((0 != test_strcpy_abcd_strncmp_4 ("abcd")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 205, "0 != test_strcpy_abcd_strncmp_4 (\"abcd\")"), __builtin_abort ()));
  ((0 != test_strcpy_abcd_strncmp_4 ("abcde")) ? (void)0 : (__builtin_printf ("assertion failed on line %i: %s\n", 206, "0 != test_strcpy_abcd_strncmp_4 (\"abcde\")"), __builtin_abort ()));
}

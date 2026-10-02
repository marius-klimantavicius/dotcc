//type: fp
//options: 
# 0 "./pr79214.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr79214.c"





# 1 "./pr79214.h" 1
       
# 2 "./pr79214.h" 3
# 7 "./pr79214.c" 2

typedef long unsigned int size_t;

char d[3];
char s[4];

static size_t range (void)
{
  extern size_t size ();
  size_t n = size ();
  if (n <= sizeof d)
    return sizeof d + 1;

  return n;
}

void test_bzero (void)
{
  
# 25 "./pr79214.c" 3
 __builtin_bzero 
# 25 "./pr79214.c"
       (d, range ());
}

void test_memcpy (void)
{
  
# 30 "./pr79214.c" 3
 __builtin_memcpy 
# 30 "./pr79214.c"
        (d, s, range ());
}

void test_memmove (void)
{
  
# 35 "./pr79214.c" 3
 __builtin_memmove 
# 35 "./pr79214.c"
         (d, d + 1, range ());
}

void test_mempcpy (void)
{
  
# 40 "./pr79214.c" 3
 __builtin_mempcpy 
# 40 "./pr79214.c"
         (d, s, range ());
}

void test_memset (int n)
{
  
# 45 "./pr79214.c" 3
 __builtin_memset 
# 45 "./pr79214.c"
        (d, n, range ());
}

void test_strcat (int i)
{
  const char *s = i < 0 ? "123" : "4567";

  
# 52 "./pr79214.c" 3
 __builtin_strcat 
# 52 "./pr79214.c"
        (d, s);
}

char* test_stpcpy (int i)
{
  const char *s = i < 0 ? "123" : "4567";

  return 
# 59 "./pr79214.c" 3
        __builtin_stpcpy 
# 59 "./pr79214.c"
               (d, s);
}

char* test_stpncpy (int i)
{
  const char *s = i < 0 ? "123" : "4567";

  return 
# 66 "./pr79214.c" 3
        __builtin_stpncpy 
# 66 "./pr79214.c"
                (d, s, range ());
}

char* test_strcpy (int i)
{
  const char *s = i < 0 ? "123" : "4567";

  return 
# 73 "./pr79214.c" 3
        __builtin_strcpy 
# 73 "./pr79214.c"
               (d, s);
}

char* test_strncpy (int i)
{
  const char *s = i < 0 ? "123" : "4567";

  return 
# 80 "./pr79214.c" 3
        __builtin_strncpy 
# 80 "./pr79214.c"
                (d, s, range ());
}

char* test_strncat (int i)
{
  const char *s = i < 0 ? "123" : "4567";

  return 
# 87 "./pr79214.c" 3
        __builtin_strncat 
# 87 "./pr79214.c"
                (d, s, range ());
}

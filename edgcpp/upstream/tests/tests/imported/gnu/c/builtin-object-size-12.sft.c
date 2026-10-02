//type: rp
//options: 
# 0 "./builtin-object-size-12.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./builtin-object-size-12.c"



# 1 "./builtin-object-size-common.h" 1
typedef long unsigned int size_t;



  extern void exit (int);
  extern void *malloc (size_t);
  extern void free (void *);
  extern void *calloc (size_t, size_t);
  extern void *alloca (size_t);
  extern void *memcpy (void *, const void *, size_t);
  extern void *memset (void *, int, size_t);
  extern char *strcpy (char *, const char *);
  extern char *strdup (const char *);
  extern char *strndup (const char *, size_t);




unsigned nfails = 0;
# 5 "./builtin-object-size-12.c" 2

struct S {
    int len;
    char s[0];
};
int main()
{
  char buf[sizeof (struct S) + 32];
  if (__builtin_object_size (((struct S *)&buf[0])->s, 1) != 32)
    do { __builtin_printf ("Failure at line: %d\n", 14); nfails++; } while (0);
  if (__builtin_object_size (((struct S *)&buf[1])->s, 1) != 31)
    do { __builtin_printf ("Failure at line: %d\n", 16); nfails++; } while (0);
  if (__builtin_object_size (((struct S *)&buf[64])->s, 0) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 18); nfails++; } while (0);

  do { if (nfails > 0) __builtin_abort (); return 0; } while (0);
}

//type: rp
//options: 
# 0 "./builtin-object-size-15.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./builtin-object-size-15.c"



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
# 5 "./builtin-object-size-15.c" 2

int
main ()
{
  struct A { char buf1[9]; char buf2[1]; } a;

  if (__builtin_object_size (a.buf1 + (0 + 4), 1) != 5)
    do { __builtin_printf ("Failure at line: %d\n", 12); nfails++; } while (0);
  char *p = a.buf1;
  p += 1;
  p += 3;
  if (__builtin_object_size (p, 1) != 5)
    do { __builtin_printf ("Failure at line: %d\n", 17); nfails++; } while (0);
  p = (char *) &a;
  char *q = p + 1;
  char *r = q + 3;
  char *t = r;
  if (r != (char *) &a + 4)
    t = (char *) &a + 1;
  if (__builtin_object_size (t, 1) != 6)
    do { __builtin_printf ("Failure at line: %d\n", 25); nfails++; } while (0);

  do { if (nfails > 0) __builtin_abort (); return 0; } while (0);
}

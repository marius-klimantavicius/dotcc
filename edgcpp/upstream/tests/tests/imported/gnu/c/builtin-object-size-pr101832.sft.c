//type: rp
//options: 
# 0 "./builtin-object-size-pr101832.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./builtin-object-size-pr101832.c"
# 11 "./builtin-object-size-pr101832.c"
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
# 12 "./builtin-object-size-pr101832.c" 2
# 24 "./builtin-object-size-pr101832.c"
struct A {
  int n;
  char data[];
};

struct B {
  int m;
  struct A a;
};

struct C {
  int q;
  struct B b;
};

struct A0 {
  int n;
  char data[0];
};

struct B0 {
  int m;
  struct A0 a;
};

struct C0 {
  int q;
  struct B0 b;
};

struct A1 {
  int n;
  char data[1];
};

struct B1 {
  int m;
  struct A1 a;
};

struct C1 {
  int q;
  struct B1 b;
};

struct An {
  int n;
  char data[8];
};

struct Bn {
  int m;
  struct An a;
};

struct Cn {
  int q;
  struct Bn b;
};

volatile void *magic1, *magic2;

int main (int argc, char *argv[])
{
  struct B *outer;
  struct C *outest;


  outer = (void *)magic1;
  outest = (void *)magic2;

  do { size_t v = -1; if (__builtin_object_size (&outer->a, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_object_size (&outer->a, 1)", __builtin_object_size (&outer->a, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_object_size (&outer->a, 1)", __builtin_object_size (&outer->a, 1), v); do { __builtin_printf ("Failure at line: %d\n", 95); nfails++; } while (0); } } while (0);;
  do { size_t v = -1; if (__builtin_object_size (&outest->b, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_object_size (&outest->b, 1)", __builtin_object_size (&outest->b, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_object_size (&outest->b, 1)", __builtin_object_size (&outest->b, 1), v); do { __builtin_printf ("Failure at line: %d\n", 96); nfails++; } while (0); } } while (0);;
  do { size_t v = -1; if (__builtin_object_size (&outest->b.a, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_object_size (&outest->b.a, 1)", __builtin_object_size (&outest->b.a, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_object_size (&outest->b.a, 1)", __builtin_object_size (&outest->b.a, 1), v); do { __builtin_printf ("Failure at line: %d\n", 97); nfails++; } while (0); } } while (0);;

  struct B0 *outer0;
  struct C0 *outest0;


  outer0 = (void *)magic1;
  outest0 = (void *)magic2;

  do { size_t v = sizeof (outer0->a); if (__builtin_object_size (&outer0->a, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_object_size (&outer0->a, 1)", __builtin_object_size (&outer0->a, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_object_size (&outer0->a, 1)", __builtin_object_size (&outer0->a, 1), v); do { __builtin_printf ("Failure at line: %d\n", 106); nfails++; } while (0); } } while (0);;
  do { size_t v = sizeof (outest0->b); if (__builtin_object_size (&outest0->b, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_object_size (&outest0->b, 1)", __builtin_object_size (&outest0->b, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_object_size (&outest0->b, 1)", __builtin_object_size (&outest0->b, 1), v); do { __builtin_printf ("Failure at line: %d\n", 107); nfails++; } while (0); } } while (0);;
  do { size_t v = sizeof (outest0->b.a); if (__builtin_object_size (&outest0->b.a, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_object_size (&outest0->b.a, 1)", __builtin_object_size (&outest0->b.a, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_object_size (&outest0->b.a, 1)", __builtin_object_size (&outest0->b.a, 1), v); do { __builtin_printf ("Failure at line: %d\n", 108); nfails++; } while (0); } } while (0);;

  struct B1 *outer1;
  struct C1 *outest1;


  outer1 = (void *)magic1;
  outest1 = (void *)magic2;

  do { size_t v = sizeof (outer1->a); if (__builtin_object_size (&outer1->a, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_object_size (&outer1->a, 1)", __builtin_object_size (&outer1->a, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_object_size (&outer1->a, 1)", __builtin_object_size (&outer1->a, 1), v); do { __builtin_printf ("Failure at line: %d\n", 117); nfails++; } while (0); } } while (0);;
  do { size_t v = sizeof (outest1->b); if (__builtin_object_size (&outest1->b, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_object_size (&outest1->b, 1)", __builtin_object_size (&outest1->b, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_object_size (&outest1->b, 1)", __builtin_object_size (&outest1->b, 1), v); do { __builtin_printf ("Failure at line: %d\n", 118); nfails++; } while (0); } } while (0);;
  do { size_t v = sizeof (outest1->b.a); if (__builtin_object_size (&outest1->b.a, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_object_size (&outest1->b.a, 1)", __builtin_object_size (&outest1->b.a, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_object_size (&outest1->b.a, 1)", __builtin_object_size (&outest1->b.a, 1), v); do { __builtin_printf ("Failure at line: %d\n", 119); nfails++; } while (0); } } while (0);;

  struct Bn *outern;
  struct Cn *outestn;


  outern = (void *)magic1;
  outestn = (void *)magic2;

  do { size_t v = sizeof (outern->a); if (__builtin_object_size (&outern->a, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_object_size (&outern->a, 1)", __builtin_object_size (&outern->a, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_object_size (&outern->a, 1)", __builtin_object_size (&outern->a, 1), v); do { __builtin_printf ("Failure at line: %d\n", 128); nfails++; } while (0); } } while (0);;
  do { size_t v = sizeof (outestn->b); if (__builtin_object_size (&outestn->b, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_object_size (&outestn->b, 1)", __builtin_object_size (&outestn->b, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_object_size (&outestn->b, 1)", __builtin_object_size (&outestn->b, 1), v); do { __builtin_printf ("Failure at line: %d\n", 129); nfails++; } while (0); } } while (0);;
  do { size_t v = sizeof (outestn->b.a); if (__builtin_object_size (&outestn->b.a, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_object_size (&outestn->b.a, 1)", __builtin_object_size (&outestn->b.a, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_object_size (&outestn->b.a, 1)", __builtin_object_size (&outestn->b.a, 1), v); do { __builtin_printf ("Failure at line: %d\n", 130); nfails++; } while (0); } } while (0);;

  do { if (nfails > 0) __builtin_abort (); return 0; } while (0);
  return 0;
}

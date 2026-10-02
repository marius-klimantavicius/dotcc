//type: rp
//options: 
# 0 "./pr101836.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr101836.c"






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
# 8 "./pr101836.c" 2
# 20 "./pr101836.c"
struct trailing_array_1 {
    int a;
    int b;
    int c[4];
};

struct trailing_array_2 {
    int a;
    int b;
    int c[1];
};

struct trailing_array_3 {
    int a;
    int b;
    int c[0];
};
struct trailing_array_4 {
    int a;
    int b;
    int c[];
};

void __attribute__((__noinline__)) stuff(
    struct trailing_array_1 *normal,
    struct trailing_array_2 *trailing_1,
    struct trailing_array_3 *trailing_0,
    struct trailing_array_4 *trailing_flex)
{
    do { size_t v = 4 * 4; if (__builtin_object_size(normal->c, 1) == v) __builtin_printf("ok:  %s == %zd\n", "__builtin_object_size(normal->c, 1)", __builtin_object_size(normal->c, 1)); else { __builtin_printf("WAT: %s == %zd (expected %zd)\n", "__builtin_object_size(normal->c, 1)", __builtin_object_size(normal->c, 1), v); do { __builtin_printf ("Failure at line: %d\n", 49); nfails++; } while (0); } } while (0);;
    do { size_t v = 4; if (__builtin_object_size(trailing_1->c, 1) == v) __builtin_printf("ok:  %s == %zd\n", "__builtin_object_size(trailing_1->c, 1)", __builtin_object_size(trailing_1->c, 1)); else { __builtin_printf("WAT: %s == %zd (expected %zd)\n", "__builtin_object_size(trailing_1->c, 1)", __builtin_object_size(trailing_1->c, 1), v); do { __builtin_printf ("Failure at line: %d\n", 50); nfails++; } while (0); } } while (0);;
    do { size_t v = 0; if (__builtin_object_size(trailing_0->c, 1) == v) __builtin_printf("ok:  %s == %zd\n", "__builtin_object_size(trailing_0->c, 1)", __builtin_object_size(trailing_0->c, 1)); else { __builtin_printf("WAT: %s == %zd (expected %zd)\n", "__builtin_object_size(trailing_0->c, 1)", __builtin_object_size(trailing_0->c, 1), v); do { __builtin_printf ("Failure at line: %d\n", 51); nfails++; } while (0); } } while (0);;
    do { size_t v = -1; if (__builtin_object_size(trailing_flex->c, 1) == v) __builtin_printf("ok:  %s == %zd\n", "__builtin_object_size(trailing_flex->c, 1)", __builtin_object_size(trailing_flex->c, 1)); else { __builtin_printf("WAT: %s == %zd (expected %zd)\n", "__builtin_object_size(trailing_flex->c, 1)", __builtin_object_size(trailing_flex->c, 1), v); do { __builtin_printf ("Failure at line: %d\n", 52); nfails++; } while (0); } } while (0);;
}

int main(int argc, char *argv[])
{
    stuff((void *)argv[0], (void *)argv[0], (void *)argv[0], (void *)argv[0]);

    do { if (nfails > 0) __builtin_abort (); return 0; } while (0);
}

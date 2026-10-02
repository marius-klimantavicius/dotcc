//type: rp
//options: 
# 0 "./flex-array-counted-by-5.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./flex-array-counted-by-5.c"





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
# 7 "./flex-array-counted-by-5.c" 2

struct annotated {
  int b;
  int c[] __attribute__ ((counted_by (b)));
} *array_annotated;

struct nested_annotated {
  struct {
    union {
      int b;
      float f;
    };
    int n;
  };
  int c[] __attribute__ ((counted_by (b)));
} *array_nested_annotated;

void __attribute__((__noinline__)) setup (int attr_count)
{
  array_annotated
    = (struct annotated *)malloc (sizeof (struct annotated));
  array_annotated->b = attr_count;

  array_nested_annotated
    = (struct nested_annotated *)malloc (sizeof (struct nested_annotated));
  array_nested_annotated->b = attr_count -1;

  return;
}

void __attribute__((__noinline__)) test ()
{
    do { size_t v = 0; if (__builtin_dynamic_object_size(array_annotated->c, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(array_annotated->c, 1)", __builtin_dynamic_object_size(array_annotated->c, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(array_annotated->c, 1)", __builtin_dynamic_object_size(array_annotated->c, 1), v); do { __builtin_printf ("Failure at line: %d\n", 39); nfails++; } while (0); } } while (0);;
    do { size_t v = 0; if (__builtin_dynamic_object_size(array_nested_annotated->c, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(array_nested_annotated->c, 1)", __builtin_dynamic_object_size(array_nested_annotated->c, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(array_nested_annotated->c, 1)", __builtin_dynamic_object_size(array_nested_annotated->c, 1), v); do { __builtin_printf ("Failure at line: %d\n", 40); nfails++; } while (0); } } while (0);;
}

int main(int argc, char *argv[])
{
  setup (-10);
  test ();
  do { if (nfails > 0) __builtin_abort (); return 0; } while (0);
}

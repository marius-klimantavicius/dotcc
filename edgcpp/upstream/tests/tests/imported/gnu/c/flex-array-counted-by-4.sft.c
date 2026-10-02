//type: rp
//options: 
# 0 "./flex-array-counted-by-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./flex-array-counted-by-4.c"
# 9 "./flex-array-counted-by-4.c"
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
# 10 "./flex-array-counted-by-4.c" 2

struct annotated {
  size_t foo;
  char others;
  char array[] __attribute__((counted_by (foo)));
};
# 52 "./flex-array-counted-by-4.c"
static struct annotated * __attribute__((__noinline__)) alloc_buf_more (size_t index)
{
  struct annotated *p;
  size_t allocated_size
    = ((sizeof (struct annotated)) > ((__builtin_offsetof (struct annotated, array[0]) + (index + 10) * sizeof (char))) ? (sizeof (struct annotated)) : ((__builtin_offsetof (struct annotated, array[0]) + (index + 10) * sizeof (char))))

                                            ;
  p = (struct annotated *) malloc (allocated_size);

  p->foo = index;
# 74 "./flex-array-counted-by-4.c"
  do { size_t v = (p->foo) * sizeof(char); if (__builtin_dynamic_object_size(p->array, 0) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p->array, 0)", __builtin_dynamic_object_size(p->array, 0)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p->array, 0)", __builtin_dynamic_object_size(p->array, 0), v); do { __builtin_printf ("Failure at line: %d\n", 74); nfails++; } while (0); } } while (0);
                          ;

  do { size_t v = (p->foo) * sizeof(char); if (__builtin_dynamic_object_size(p->array, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p->array, 1)", __builtin_dynamic_object_size(p->array, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p->array, 1)", __builtin_dynamic_object_size(p->array, 1), v); do { __builtin_printf ("Failure at line: %d\n", 77); nfails++; } while (0); } } while (0);
                          ;

  do { size_t v = (p->foo) * sizeof(char); if (__builtin_dynamic_object_size(p->array, 2) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p->array, 2)", __builtin_dynamic_object_size(p->array, 2)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p->array, 2)", __builtin_dynamic_object_size(p->array, 2), v); do { __builtin_printf ("Failure at line: %d\n", 80); nfails++; } while (0); } } while (0);
                          ;

  do { size_t v = (p->foo) * sizeof(char); if (__builtin_dynamic_object_size(p->array, 3) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p->array, 3)", __builtin_dynamic_object_size(p->array, 3)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p->array, 3)", __builtin_dynamic_object_size(p->array, 3), v); do { __builtin_printf ("Failure at line: %d\n", 83); nfails++; } while (0); } } while (0);
                          ;




  do { size_t v = allocated_size; if (__builtin_dynamic_object_size(p, 0) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p, 0)", __builtin_dynamic_object_size(p, 0)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p, 0)", __builtin_dynamic_object_size(p, 0), v); do { __builtin_printf ("Failure at line: %d\n", 89); nfails++; } while (0); } } while (0);;
  do { size_t v = allocated_size; if (__builtin_dynamic_object_size(p, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p, 1)", __builtin_dynamic_object_size(p, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p, 1)", __builtin_dynamic_object_size(p, 1), v); do { __builtin_printf ("Failure at line: %d\n", 90); nfails++; } while (0); } } while (0);;
  do { size_t v = allocated_size; if (__builtin_dynamic_object_size(p, 2) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p, 2)", __builtin_dynamic_object_size(p, 2)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p, 2)", __builtin_dynamic_object_size(p, 2), v); do { __builtin_printf ("Failure at line: %d\n", 91); nfails++; } while (0); } } while (0);;
  do { size_t v = allocated_size; if (__builtin_dynamic_object_size(p, 3) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p, 3)", __builtin_dynamic_object_size(p, 3)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p, 3)", __builtin_dynamic_object_size(p, 3), v); do { __builtin_printf ("Failure at line: %d\n", 92); nfails++; } while (0); } } while (0);;
  return p;
}







static struct annotated * __attribute__((__noinline__)) alloc_buf_less (size_t index)
{
  struct annotated *p;
  size_t allocated_size
    = ((sizeof (struct annotated)) > ((__builtin_offsetof (struct annotated, array[0]) + (index) * sizeof (char))) ? (sizeof (struct annotated)) : ((__builtin_offsetof (struct annotated, array[0]) + (index) * sizeof (char))))

                                ;
  p = (struct annotated *) malloc (allocated_size);

  p->foo = index + 10;
# 123 "./flex-array-counted-by-4.c"
  do { size_t v = (p->foo) * sizeof(char); if (__builtin_dynamic_object_size(p->array, 0) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p->array, 0)", __builtin_dynamic_object_size(p->array, 0)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p->array, 0)", __builtin_dynamic_object_size(p->array, 0), v); do { __builtin_printf ("Failure at line: %d\n", 123); nfails++; } while (0); } } while (0);
                          ;

  do { size_t v = (p->foo) * sizeof(char); if (__builtin_dynamic_object_size(p->array, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p->array, 1)", __builtin_dynamic_object_size(p->array, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p->array, 1)", __builtin_dynamic_object_size(p->array, 1), v); do { __builtin_printf ("Failure at line: %d\n", 126); nfails++; } while (0); } } while (0);
                          ;

  do { size_t v = (p->foo) * sizeof(char); if (__builtin_dynamic_object_size(p->array, 2) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p->array, 2)", __builtin_dynamic_object_size(p->array, 2)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p->array, 2)", __builtin_dynamic_object_size(p->array, 2), v); do { __builtin_printf ("Failure at line: %d\n", 129); nfails++; } while (0); } } while (0);
                          ;

  do { size_t v = (p->foo) * sizeof(char); if (__builtin_dynamic_object_size(p->array, 3) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p->array, 3)", __builtin_dynamic_object_size(p->array, 3)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p->array, 3)", __builtin_dynamic_object_size(p->array, 3), v); do { __builtin_printf ("Failure at line: %d\n", 132); nfails++; } while (0); } } while (0);
                          ;




  do { size_t v = allocated_size; if (__builtin_dynamic_object_size(p, 0) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p, 0)", __builtin_dynamic_object_size(p, 0)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p, 0)", __builtin_dynamic_object_size(p, 0), v); do { __builtin_printf ("Failure at line: %d\n", 138); nfails++; } while (0); } } while (0);;
  do { size_t v = allocated_size; if (__builtin_dynamic_object_size(p, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p, 1)", __builtin_dynamic_object_size(p, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p, 1)", __builtin_dynamic_object_size(p, 1), v); do { __builtin_printf ("Failure at line: %d\n", 139); nfails++; } while (0); } } while (0);;
  do { size_t v = allocated_size; if (__builtin_dynamic_object_size(p, 2) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p, 2)", __builtin_dynamic_object_size(p, 2)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p, 2)", __builtin_dynamic_object_size(p, 2), v); do { __builtin_printf ("Failure at line: %d\n", 140); nfails++; } while (0); } } while (0);;
  do { size_t v = allocated_size; if (__builtin_dynamic_object_size(p, 3) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p, 3)", __builtin_dynamic_object_size(p, 3)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p, 3)", __builtin_dynamic_object_size(p, 3), v); do { __builtin_printf ("Failure at line: %d\n", 141); nfails++; } while (0); } } while (0);;
  return p;
}

int main ()
{
  struct annotated *p, *q;
  p = alloc_buf_more (10);
  q = alloc_buf_less (10);



  do { size_t v = p->foo * sizeof(char); if (__builtin_dynamic_object_size(p->array, 0) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p->array, 0)", __builtin_dynamic_object_size(p->array, 0)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p->array, 0)", __builtin_dynamic_object_size(p->array, 0), v); do { __builtin_printf ("Failure at line: %d\n", 153); nfails++; } while (0); } } while (0);;
  do { size_t v = p->foo * sizeof(char); if (__builtin_dynamic_object_size(p->array, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p->array, 1)", __builtin_dynamic_object_size(p->array, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p->array, 1)", __builtin_dynamic_object_size(p->array, 1), v); do { __builtin_printf ("Failure at line: %d\n", 154); nfails++; } while (0); } } while (0);;
  do { size_t v = p->foo * sizeof(char); if (__builtin_dynamic_object_size(p->array, 2) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p->array, 2)", __builtin_dynamic_object_size(p->array, 2)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p->array, 2)", __builtin_dynamic_object_size(p->array, 2), v); do { __builtin_printf ("Failure at line: %d\n", 155); nfails++; } while (0); } } while (0);;
  do { size_t v = p->foo * sizeof(char); if (__builtin_dynamic_object_size(p->array, 3) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p->array, 3)", __builtin_dynamic_object_size(p->array, 3)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p->array, 3)", __builtin_dynamic_object_size(p->array, 3), v); do { __builtin_printf ("Failure at line: %d\n", 156); nfails++; } while (0); } } while (0);;


  do { size_t v = -1; if (__builtin_dynamic_object_size(p, 0) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p, 0)", __builtin_dynamic_object_size(p, 0)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p, 0)", __builtin_dynamic_object_size(p, 0), v); do { __builtin_printf ("Failure at line: %d\n", 159); nfails++; } while (0); } } while (0);;
  do { size_t v = -1; if (__builtin_dynamic_object_size(p, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p, 1)", __builtin_dynamic_object_size(p, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p, 1)", __builtin_dynamic_object_size(p, 1), v); do { __builtin_printf ("Failure at line: %d\n", 160); nfails++; } while (0); } } while (0);;
  do { size_t v = 0; if (__builtin_dynamic_object_size(p, 2) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p, 2)", __builtin_dynamic_object_size(p, 2)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p, 2)", __builtin_dynamic_object_size(p, 2), v); do { __builtin_printf ("Failure at line: %d\n", 161); nfails++; } while (0); } } while (0);;
  do { size_t v = 0; if (__builtin_dynamic_object_size(p, 3) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(p, 3)", __builtin_dynamic_object_size(p, 3)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(p, 3)", __builtin_dynamic_object_size(p, 3), v); do { __builtin_printf ("Failure at line: %d\n", 162); nfails++; } while (0); } } while (0);;



  do { size_t v = q->foo * sizeof(char); if (__builtin_dynamic_object_size(q->array, 0) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(q->array, 0)", __builtin_dynamic_object_size(q->array, 0)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(q->array, 0)", __builtin_dynamic_object_size(q->array, 0), v); do { __builtin_printf ("Failure at line: %d\n", 166); nfails++; } while (0); } } while (0);;
  do { size_t v = q->foo * sizeof(char); if (__builtin_dynamic_object_size(q->array, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(q->array, 1)", __builtin_dynamic_object_size(q->array, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(q->array, 1)", __builtin_dynamic_object_size(q->array, 1), v); do { __builtin_printf ("Failure at line: %d\n", 167); nfails++; } while (0); } } while (0);;
  do { size_t v = q->foo * sizeof(char); if (__builtin_dynamic_object_size(q->array, 2) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(q->array, 2)", __builtin_dynamic_object_size(q->array, 2)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(q->array, 2)", __builtin_dynamic_object_size(q->array, 2), v); do { __builtin_printf ("Failure at line: %d\n", 168); nfails++; } while (0); } } while (0);;
  do { size_t v = q->foo * sizeof(char); if (__builtin_dynamic_object_size(q->array, 3) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(q->array, 3)", __builtin_dynamic_object_size(q->array, 3)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(q->array, 3)", __builtin_dynamic_object_size(q->array, 3), v); do { __builtin_printf ("Failure at line: %d\n", 169); nfails++; } while (0); } } while (0);;


  do { size_t v = -1; if (__builtin_dynamic_object_size(q, 0) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(q, 0)", __builtin_dynamic_object_size(q, 0)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(q, 0)", __builtin_dynamic_object_size(q, 0), v); do { __builtin_printf ("Failure at line: %d\n", 172); nfails++; } while (0); } } while (0);;
  do { size_t v = -1; if (__builtin_dynamic_object_size(q, 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(q, 1)", __builtin_dynamic_object_size(q, 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(q, 1)", __builtin_dynamic_object_size(q, 1), v); do { __builtin_printf ("Failure at line: %d\n", 173); nfails++; } while (0); } } while (0);;
  do { size_t v = 0; if (__builtin_dynamic_object_size(q, 2) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(q, 2)", __builtin_dynamic_object_size(q, 2)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(q, 2)", __builtin_dynamic_object_size(q, 2), v); do { __builtin_printf ("Failure at line: %d\n", 174); nfails++; } while (0); } } while (0);;
  do { size_t v = 0; if (__builtin_dynamic_object_size(q, 3) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size(q, 3)", __builtin_dynamic_object_size(q, 3)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size(q, 3)", __builtin_dynamic_object_size(q, 3), v); do { __builtin_printf ("Failure at line: %d\n", 175); nfails++; } while (0); } } while (0);;

  do { if (nfails > 0) __builtin_abort (); return 0; } while (0);
}

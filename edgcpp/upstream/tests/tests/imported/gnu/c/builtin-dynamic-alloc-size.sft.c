//type: fp
//options: 
# 0 "./builtin-dynamic-alloc-size.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./builtin-dynamic-alloc-size.c"




# 1 "./builtin-alloc-size.c" 1







void sink (void*);

static unsigned size (unsigned n)
{
  return n;
}

void test_aligned_alloc (unsigned a)
{
  unsigned n = size (7);

  void *p = __builtin_aligned_alloc (a, n);
  if (__builtin_dynamic_object_size (p, 0) != n)
    __builtin_abort ();
  sink (p);
}

void test_alloca (void)
{
  unsigned n = size (13);

  void *p = __builtin_alloca (n);



  if (!p)
    __builtin_abort ();

  if (__builtin_dynamic_object_size (p, 0) != n)
    __builtin_abort ();
  sink (p);
}

void test_calloc (void)
{
  unsigned m = size (19);
  unsigned n = size (23);

  void *p = __builtin_calloc (m, n);
  if (__builtin_dynamic_object_size (p, 0) != m * n)
    __builtin_abort ();
  sink (p);
}

void test_malloc (void)
{
  unsigned n = size (17);

  void *p = __builtin_malloc (n);
  if (__builtin_dynamic_object_size (p, 0) != n)
    __builtin_abort ();
  sink (p);
}

void test_realloc (void *p)
{
  unsigned n = size (31);

  p = __builtin_realloc (p, n);
  if (__builtin_dynamic_object_size (p, 0) != n)
    __builtin_abort ();
  sink (p);
}
# 6 "./builtin-dynamic-alloc-size.c" 2

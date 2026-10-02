//source_files: builtin-dynamic-object-size-5-main.c
//type: rp
//options: 
# 0 "./builtin-dynamic-object-size-5.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./builtin-dynamic-object-size-5.c"





# 1 "./builtin-object-size-5.c" 1







typedef long unsigned int size_t;
extern void abort (void);
extern char buf[0x40000000];

void
test1 (size_t x)
{
  char *p = &buf[8];
  size_t i;

  for (i = 0; i < x; ++i)
    p = p + 4;

  if (__builtin_dynamic_object_size (p, 0) != sizeof (buf) - 8 - 4 * x)



    abort ();
}

void
test2 (size_t x)
{
  char *p = &buf[8];
  size_t i;

  for (i = 0; i < x; ++i)
    p = p + 4;

  if (__builtin_dynamic_object_size (p, 1) != sizeof (buf) - 8 - 4 * x)



    abort ();
}

void
test3 (size_t x)
{
  char *p = &buf[8];
  size_t i;

  for (i = 0; i < x; ++i)
    p = p + 4;

  if (__builtin_dynamic_object_size (p, 2) != sizeof (buf) - 8 - 4 * x)



    abort ();
}

void
test4 (size_t x)
{
  char *p = &buf[8];
  size_t i;

  for (i = 0; i < x; ++i)
    p = p + 4;

  if (__builtin_dynamic_object_size (p, 3) != sizeof (buf) - 8 - 4 * x)



    abort ();
}

void
test5 (void)
{
  char *p = &buf[0x90000004];
  if (__builtin_dynamic_object_size (p + 2, 0) != 0)
    abort ();
}

void
test6 (void)
{
  char *p = &buf[-4];
  if (__builtin_dynamic_object_size (p + 2, 0) != 0)
    abort ();
}


void
test7 (void)
{
  char *buf2 = __builtin_malloc (8);
  char *p = &buf2[0x90000004];
  if (__builtin_dynamic_object_size (p + 2, 0) != 0)
    abort ();
}
# 7 "./builtin-dynamic-object-size-5.c" 2

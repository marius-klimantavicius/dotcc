//type: fp
//options: 
# 0 "./analyzer/torture/boxed-int-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/torture/boxed-int-1.c"


# 1 "./analyzer/torture/../analyzer-decls.h" 1
# 20 "./analyzer/torture/../analyzer-decls.h"
extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 44 "./analyzer/torture/../analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);




extern long unsigned int __analyzer_get_strlen (const char *ptr);
# 4 "./analyzer/torture/boxed-int-1.c" 2

typedef struct boxed_int { int value; } boxed_int;

extern boxed_int boxed_int_add (boxed_int a, boxed_int b);
extern boxed_int boxed_int_mul (boxed_int a, boxed_int b);

boxed_int __attribute__((noinline))
noinline_boxed_int_add (boxed_int a, boxed_int b)
{
  boxed_int result;
  result.value = a.value + b.value;
  return result;
}

static inline boxed_int
inline_boxed_int_add (boxed_int a, boxed_int b)
{
  boxed_int result;
  result.value = a.value + b.value;
  return result;
}

boxed_int
test_1 (boxed_int a, boxed_int b)
{
  boxed_int result = boxed_int_add (boxed_int_mul (a, a),
        boxed_int_mul (b, b));
  return result;
}

void
test_2a (void)
{
  boxed_int arr[4];
  arr[0].value = 1;
  arr[1].value = 2;
  arr[2].value = 3;
  arr[3].value = 4;
  boxed_int sum;
  sum.value = arr[0].value + arr[1].value + arr[2].value + arr[3].value;
  __analyzer_eval (sum.value == 10);
}

void
test_2b (void)
{
  boxed_int a, b, c, d;
  a.value = 1;
  b.value = 2;
  c.value = 3;
  d.value = 4;
  boxed_int sum;
  sum.value = a.value + b.value + c.value + d.value;
  __analyzer_eval (sum.value == 10);
}

void
test_2c (void)
{
  boxed_int a, b, c, d;
  a.value = 1;
  b.value = 2;
  c.value = 3;
  d.value = 4;
  boxed_int sum = inline_boxed_int_add (inline_boxed_int_add (a, b),
     inline_boxed_int_add (c, d));
  __analyzer_eval (sum.value == 10);
}

void
test_2d (void)
{
  boxed_int a, b, c, d;
  a.value = 1;
  b.value = 2;
  c.value = 3;
  d.value = 4;
  boxed_int sum = noinline_boxed_int_add (noinline_boxed_int_add (a, b),
       noinline_boxed_int_add (c, d));
  __analyzer_eval (sum.value == 10);
}



void test_4 (void)
{
  boxed_int i;
  int *p = &i.value;
  i.value = 1;
  *p = 2;
  __analyzer_eval (i.value == 2);
}



void test_5 (void)
{
  boxed_int a[10];
  a[3].value = 5;
  __analyzer_eval (a[3].value == 5);
}



void test_5a (int idx)
{
  boxed_int a[10];
  a[idx].value = 5;
  __analyzer_eval (a[idx].value == 5);
}



void test_6 (boxed_int a[10])
{

  __analyzer_eval (a[3].value == 42);
  a[3].value = 42;
  __analyzer_eval (a[3].value == 42);
}



void test_7 (boxed_int *a)
{
  __analyzer_eval (a[3].value == 42);
  a[3].value = 42;
  __analyzer_eval (a[3].value == 42);
}



boxed_int glob_a;

void test_10 (void)
{
  __analyzer_eval (glob_a.value == 42);
  glob_a.value = 42;
  __analyzer_eval (glob_a.value == 42);
}


int test_12a (void)
{
  boxed_int i;
  return i.value;
}


boxed_int test_12b (void)
{
  boxed_int i;
  return i;
}

void test_loop (void)
{
  boxed_int i;

  __analyzer_dump_exploded_nodes (0);

  for (i.value=0; i.value<256; i.value++) {
      __analyzer_dump_exploded_nodes (0);
  }

  __analyzer_dump_exploded_nodes (0);
}

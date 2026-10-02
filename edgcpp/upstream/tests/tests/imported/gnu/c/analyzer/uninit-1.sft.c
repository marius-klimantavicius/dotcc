//type: fp
//options: 
# 0 "./analyzer/uninit-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/uninit-1.c"
# 1 "./analyzer/analyzer-decls.h" 1







extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 32 "./analyzer/analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);
# 2 "./analyzer/uninit-1.c" 2
typedef long unsigned int size_t;

int test_1 (void)
{
  int i;
  return i;
}

int test_2 (void)
{
  int i;
  return i * 2;
}

int test_3 (void)
{
  static int i;
  return i;
}

int test_4 (void)
{
  int *p;
  return *p;
}

int test_5 (int flag, int *q)
{
  int *p;
  if (flag)
    p = q;



  __analyzer_dump_exploded_nodes (0);

  return *p;
}

int test_6 (int i)
{
  int arr[10];
  return arr[i];
}

int test_rshift_rhs (int i)
{
  int j;
  return i >> j;
}

int test_lshift_rhs (int i)
{
  int j;
  return i << j;
}

int test_rshift_lhs (int i)
{
  int j;
  return j >> i;
}

int test_lshift_lhs (int i)
{
  int j;
  return j << i;
}

int test_cmp (int i)
{
  int j;
  return i < j;
}

float test_plus_rhs (float x)
{
  float y;
  return x + y;
}

float test_plus_lhs (float x)
{
  float y;
  return y + x;
}

float test_minus_rhs (float x)
{
  float y;
  return x - y;
}

float test_minus_lhs (float x)
{
  float y;
  return y - x;
}

float test_times_rhs (float x)
{
  float y;
  return x * y;
}

float test_times_lhs (float x)
{
  float y;
  return y * x;
}

float test_divide_rhs (float x)
{
  float y;
  return x / y;
}

float test_divide_lhs (float x)
{
  float y;
  return y / x;
}

size_t test_builtin_strlen (void)
{
  const char *ptr;
  return __builtin_strlen (ptr);
}

void test_calling_uninit_fn_ptr_1 (void)
{
  void (*fn_ptr) (void);
  fn_ptr ();
}

int test_calling_uninit_fn_ptr_2 (void)
{
  int (*fn_ptr) (void);
  return fn_ptr ();
}

extern void called_by_uninit_arg (int);
void test_passing_uninit_arg (void)
{
  int i;
  called_by_uninit_arg (i);
}

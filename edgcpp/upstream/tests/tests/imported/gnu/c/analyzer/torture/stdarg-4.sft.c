//type: fp
//options: 
# 0 "./analyzer/torture/stdarg-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/torture/stdarg-4.c"


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
# 4 "./analyzer/torture/stdarg-4.c" 2



int test_1a (const char *fmt, ...)
{
  __builtin_va_list args;
  int sum = 0;
  char ch;

  __builtin_va_start(args, fmt);

  while (ch = *fmt++)
    if (ch == '%')
      sum += __builtin_va_arg(args, int);

  __builtin_va_end(args);

  return sum;
}



static int test_1b_callee (const char *fmt, __builtin_va_list args)
{
  int sum = 0;
  char ch;

  while (ch = *fmt++)
    if (ch == '%')
      sum += __builtin_va_arg(args, int);

  return sum;
}

int test_1b_caller (const char *fmt, ...)
{
  __builtin_va_list args;
  int sum = 0;

  __builtin_va_start(args, fmt);

  sum = test_1b_callee (fmt, args);

  __builtin_va_end(args);

  return sum;
}




static int
test_1c_inner (const char *fmt, __builtin_va_list args)
{
  int sum = 0;
  char ch;

  while (ch = *fmt++)
    if (ch == '%')
      sum += __builtin_va_arg(args, int);

  return sum;
}

static int
test_1c_middle (const char *fmt, ...)
{
  __builtin_va_list args;
  int sum = 0;

  __builtin_va_start(args, fmt);

  sum = test_1c_inner (fmt, args);

  __builtin_va_end(args);

  return sum;
}

void test_1c_outer (void)
{
  int sum = test_1c_middle ("%%", 42, 17);

  __analyzer_describe (0, sum);
}



int test_2a (int count, ...)
{
  __builtin_va_list args;
  int sum = 0;
  char ch;

  __builtin_va_start(args, count);

  while (count-- > 0)
    sum += __builtin_va_arg(args, int);

  __builtin_va_end(args);

  return sum;
}



static int test_2b_callee (int count, __builtin_va_list args)
{
  int sum = 0;

  while (count-- > 0)
    sum += __builtin_va_arg(args, int);

  return sum;
}

int test_2b_caller (int count, ...)
{
  __builtin_va_list args;
  int sum = 0;

  __builtin_va_start(args, count);

  sum = test_2b_callee (count, args);

  __builtin_va_end(args);

  return sum;
}




static int test_2c_inner (int count, __builtin_va_list args)
{
  int sum = 0;

  while (count-- > 0)
    sum += __builtin_va_arg(args, int);

  return sum;
}

int test_2c_middle (int count, ...)
{
  __builtin_va_list args;
  int sum = 0;

  __builtin_va_start(args, count);

  sum = test_2c_inner (count, args);

  __builtin_va_end(args);

  return sum;
}

void test_2c_outer (void)
{
  int sum = test_2c_middle (2, 50, 42);

  __analyzer_describe (0, sum);
}



int test_3a (int placeholder, ...)
{
  __builtin_va_list args;
  int sum = 0;
  int val;

  __builtin_va_start(args, placeholder);

  while (val = __builtin_va_arg(args, int))
    sum += val;

  __builtin_va_end(args);

  return sum;
}



static int test_3b_callee (__builtin_va_list args)
{
  int sum = 0;
  int val;
  while (val = __builtin_va_arg(args, int))
    sum += val;
  return sum;
}

int test_3b_caller (int placeholder, ...)
{
  __builtin_va_list args;
  int sum = 0;

  __builtin_va_start(args, placeholder);

  sum = test_3b_callee (args);

  __builtin_va_end(args);

  return sum;
}




static int test_3c_inner (__builtin_va_list args)
{
  int sum = 0;
  int val;
  while (val = __builtin_va_arg(args, int))
    sum += val;
  return sum;
}

int test_3c_middle (int placeholder, ...)
{
  __builtin_va_list args;
  int sum = 0;

  __builtin_va_start(args, placeholder);

  sum = test_3c_inner (args);

  __builtin_va_end(args);

  return sum;
}

void test_3c_outer (void)
{
  int sum = test_3c_middle (0, 5, 12, 0);
  __analyzer_describe (0, sum);
}




static int test_3d_callee (__builtin_va_list args)
{
  int sum = 0;
  int val;
  while (val = __builtin_va_arg(args, int))
    sum += val;
  return sum;
}

int test_3d_caller (int placeholder, ...)
{
  __builtin_va_list args1, args2;
  int sum = 0;

  __builtin_va_start(args1, placeholder);
  __builtin_va_copy (args2, args1);

  sum = test_3d_callee (args1);
  __builtin_va_end(args1);

  sum += test_3d_callee (args2);
  __builtin_va_end(args2);

  return sum;
}




static int test_3e_inner (__builtin_va_list args)
{
  int sum = 0;
  int val;
  while (val = __builtin_va_arg(args, int))
    sum += val;
  return sum;
}

int test_3e_middle (int placeholder, ...)
{
  __builtin_va_list args1, args2;
  int sum = 0;

  __builtin_va_start(args1, placeholder);
  __builtin_va_copy (args2, args1);

  sum = test_3e_inner (args1);
  __builtin_va_end(args1);

  sum += test_3e_inner (args2);
  __builtin_va_end(args2);

  return sum;
}

void test_3e_outer (void)
{
  int sum = test_3e_middle (0, 5, 6, 0);
  __analyzer_describe (0, sum);
}



static int test_3f_callee (int placeholder, ...)
{
  __builtin_va_list args;
  int sum = 0;
  int val;

  __builtin_va_start(args, placeholder);

  while (val = __builtin_va_arg(args, int))
    sum += val;

  __builtin_va_end(args);

  return sum;
}

void test_3f_caller (int x, int y, int z)
{
  int sum = test_3f_callee (0, x, y, z, 0);
  __analyzer_describe (0, sum);
}

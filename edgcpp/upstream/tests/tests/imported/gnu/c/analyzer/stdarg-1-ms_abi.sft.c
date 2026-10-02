//type: fp
//options: 
# 0 "./analyzer/stdarg-1-ms_abi.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/stdarg-1-ms_abi.c"





# 1 "./analyzer/analyzer-decls.h" 1
# 20 "./analyzer/analyzer-decls.h"
extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 44 "./analyzer/analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);




extern long unsigned int __analyzer_get_strlen (const char *ptr);
# 7 "./analyzer/stdarg-1-ms_abi.c" 2



static void __attribute__((noinline))
__analyzer_called_by_test_1 (int placeholder, ...)
{
  const char *s;
  int i;
  char c;

  __builtin_ms_va_list ap;
  __builtin_ms_va_start (ap, placeholder);

  s = __builtin_va_arg (ap, char *);
  __analyzer_eval (s[0] == 'f');

  i = __builtin_va_arg (ap, int);
  __analyzer_eval (i == 1066);

  c = (char)__builtin_va_arg (ap, int);
  __analyzer_eval (c == '@');

  __builtin_ms_va_end (ap);
}

void test_1 (void)
{
  __analyzer_called_by_test_1 (42, "foo", 1066, '@');
}



static void __attribute__((noinline))
__analyzer_test_2_inner (__builtin_ms_va_list ap)
{
  const char *s;
  int i;
  char c;

  s = __builtin_va_arg (ap, char *);
  __analyzer_eval (s[0] == 'f');

  i = __builtin_va_arg (ap, int);
  __analyzer_eval (i == 1066);

  c = (char)__builtin_va_arg (ap, int);
  __analyzer_eval (c == '@');
}

static void __attribute__((noinline))
__analyzer_test_2_middle (int placeholder, ...)
{
  __builtin_ms_va_list ap;
  __builtin_ms_va_start (ap, placeholder);
  __analyzer_test_2_inner (ap);
  __builtin_ms_va_end (ap);
}

void test_2 (void)
{
  __analyzer_test_2_middle (42, "foo", 1066, '@');
}



static void __attribute__((noinline))
__analyzer_called_by_test_not_enough_args (int placeholder, ...)
{
  const char *s;
  int i;

  __builtin_ms_va_list ap;
  __builtin_ms_va_start (ap, placeholder);

  s = __builtin_va_arg (ap, char *);
  __analyzer_eval (s[0] == 'f');

  i = __builtin_va_arg (ap, int);

  __builtin_ms_va_end (ap);
}

void test_not_enough_args (void)
{
  __analyzer_called_by_test_not_enough_args (42, "foo");
}



static void __attribute__((noinline))
__analyzer_test_not_enough_args_2_inner (__builtin_ms_va_list ap)
{
  const char *s;
  int i;

  s = __builtin_va_arg (ap, char *);
  __analyzer_eval (s[0] == 'f');

  i = __builtin_va_arg (ap, int);
}

static void __attribute__((noinline))
__analyzer_test_not_enough_args_2_middle (int placeholder, ...)
{
  __builtin_ms_va_list ap;
  __builtin_ms_va_start (ap, placeholder);
  __analyzer_test_not_enough_args_2_inner (ap);
  __builtin_ms_va_end (ap);
}

void test_not_enough_args_2 (void)
{
  __analyzer_test_not_enough_args_2_middle (42, "foo");
}



static void __attribute__((noinline))
__analyzer_called_by_test_excess_args (int placeholder, ...)
{
  const char *s;

  __builtin_ms_va_list ap;
  __builtin_ms_va_start (ap, placeholder);

  s = __builtin_va_arg (ap, char *);
  __analyzer_eval (s[0] == 'f');

  __builtin_ms_va_end (ap);
}

void test_excess_args (void)
{
  __analyzer_called_by_test_excess_args (42, "foo", "bar");
}



void test_missing_va_start (int placeholder, ...)
{
  __builtin_ms_va_list ap;
  int i = __builtin_va_arg (ap, int);
}



void test_missing_va_end (int placeholder, ...)
{
  int i;
  __builtin_ms_va_list ap;
  __builtin_ms_va_start (ap, placeholder);
  i = __builtin_va_arg (ap, int);
}




int test_missing_va_end_2 (int placeholder, ...)
{
  int i, j;
  __builtin_ms_va_list ap;
  __builtin_ms_va_start (ap, placeholder);
  i = __builtin_va_arg (ap, int);
  if (i == 42)
    {
      __builtin_ms_va_end (ap);
      return -1;
    }
  j = __builtin_va_arg (ap, int);
  if (j == 1066)
    return -1;
  __builtin_ms_va_end (ap);
  return 0;
}



void test_va_arg_after_va_end (int placeholder, ...)
{
  int i;
  __builtin_ms_va_list ap;
  __builtin_ms_va_start (ap, placeholder);
  __builtin_ms_va_end (ap);
  i = __builtin_va_arg (ap, int);
}



static void __attribute__((noinline))
__analyzer_called_by_test_type_mismatch_1 (int placeholder, ...)
{
  int i;

  __builtin_ms_va_list ap;
  __builtin_ms_va_start (ap, placeholder);

  i = __builtin_va_arg (ap, int);

  __builtin_ms_va_end (ap);
}

void test_type_mismatch_1 (void)
{
  __analyzer_called_by_test_type_mismatch_1 (42, "foo");
}



static void __attribute__((noinline))
__analyzer_called_by_test_type_mismatch_2 (int placeholder, ...)
{
  const char *str;

  __builtin_ms_va_list ap;
  __builtin_ms_va_start (ap, placeholder);

  str = __builtin_va_arg (ap, const char *);

  __builtin_ms_va_end (ap);
}

void test_type_mismatch_2 (void)
{
  __analyzer_called_by_test_type_mismatch_2 (42, 1066);
}



static void __attribute__((noinline))
__analyzer_test_type_mismatch_3_inner (__builtin_ms_va_list ap)
{
  const char *str;

  str = __builtin_va_arg (ap, const char *);
}

static void __attribute__((noinline))
__analyzer_test_type_mismatch_3_middle (int placeholder, ...)
{
  __builtin_ms_va_list ap;
  __builtin_ms_va_start (ap, placeholder);

  __analyzer_test_type_mismatch_3_inner (ap);

  __builtin_ms_va_end (ap);
}

void test_type_mismatch_3 (void)
{
  __analyzer_test_type_mismatch_3_middle (42, 1066);
}



static void __attribute__((noinline))
__analyzer_called_by_test_multiple_traversals (int placeholder, ...)
{
  __builtin_ms_va_list ap;


  {
    int i, j;

    __builtin_ms_va_start (ap, placeholder);

    i = __builtin_va_arg (ap, int);
    __analyzer_eval (i == 1066);

    j = __builtin_va_arg (ap, int);
    __analyzer_eval (j == 42);

    __builtin_ms_va_end (ap);
  }


  {
    int i, j;

    __builtin_ms_va_start (ap, placeholder);

    i = __builtin_va_arg (ap, int);
    __analyzer_eval (i == 1066);

    j = __builtin_va_arg (ap, int);
    __analyzer_eval (j == 42);

    __builtin_ms_va_end (ap);
  }
}

void test_multiple_traversals (void)
{
  __analyzer_called_by_test_multiple_traversals (0, 1066, 42);
}



static void __attribute__((noinline))
__analyzer_called_by_test_multiple_traversals_2 (int placeholder, ...)
{
  int i, j;
  __builtin_ms_va_list args1;
  __builtin_ms_va_list args2;

  __builtin_ms_va_start (args1, placeholder);
  __builtin_ms_va_copy (args2, args1);


  i = __builtin_va_arg (args1, int);
  __analyzer_eval (i == 1066);
  j = __builtin_va_arg (args1, int);
  __analyzer_eval (j == 42);
  __builtin_ms_va_end (args1);


  i = __builtin_va_arg (args2, int);
  __analyzer_eval (i == 1066);
  j = __builtin_va_arg (args2, int);
  __analyzer_eval (j == 42);
  __builtin_ms_va_end (args2);
}

void test_multiple_traversals_2 (void)
{
  __analyzer_called_by_test_multiple_traversals_2 (0, 1066, 42);
}



static void __attribute__((noinline))
__analyzer_called_by_test_multiple_traversals_3 (int placeholder, ...)
{
  int i, j;
  __builtin_ms_va_list args1;
  __builtin_ms_va_list args2;

  __builtin_ms_va_start (args1, placeholder);


  i = __builtin_va_arg (args1, int);
  __analyzer_eval (i == 1066);


  __builtin_ms_va_copy (args2, args1);

  j = __builtin_va_arg (args1, int);
  __analyzer_eval (j == 42);
  __builtin_ms_va_end (args1);


  j = __builtin_va_arg (args2, int);
  __analyzer_eval (j == 42);
  __builtin_ms_va_end (args2);
}

void test_multiple_traversals_3 (void)
{
  __analyzer_called_by_test_multiple_traversals_3 (0, 1066, 42);
}



void test_va_copy_after_va_end (int placeholder, ...)
{
  __builtin_ms_va_list ap1, ap2;
  __builtin_ms_va_start (ap1, placeholder);
  __builtin_ms_va_end (ap1);
  __builtin_ms_va_copy (ap2, ap1);
  __builtin_ms_va_end (ap2);
}



void test_leak_of_va_copy (int placeholder, ...)
{
  __builtin_ms_va_list ap1, ap2;
  __builtin_ms_va_start (ap1, placeholder);
  __builtin_ms_va_copy (ap2, ap1);
  __builtin_ms_va_end (ap1);
}




void test_double_va_end (int placeholder, ...)
{
  __builtin_ms_va_list ap;
  __builtin_ms_va_start (ap, placeholder);
  __builtin_ms_va_end (ap);
  __builtin_ms_va_end (ap);
}



void test_double_va_start (int placeholder, ...)
{
  int i;
  __builtin_ms_va_list ap;
  __builtin_ms_va_start (ap, placeholder);
  __builtin_ms_va_start (ap, placeholder);

  __builtin_ms_va_end (ap);
}



void test_va_copy_before_va_start (int placeholder, ...)
{
  __builtin_ms_va_list ap1;
  __builtin_ms_va_list ap2;
  __builtin_ms_va_copy (ap2, ap1);
  __builtin_ms_va_end (ap2);
}




__builtin_ms_va_list global_ap;

static void __attribute__((noinline))
__analyzer_called_by_test_va_arg_after_return (int placeholder, ...)
{
  __builtin_ms_va_start (global_ap, placeholder);
  __builtin_ms_va_end (global_ap);
}

void test_va_arg_after_return (void)
{
  int i;
  __analyzer_called_by_test_va_arg_after_return (42, 1066);
  i = __builtin_va_arg (global_ap, int);
}

void pr107349 (void)
{
  __builtin_ms_va_list x,y;
  __builtin_ms_va_copy(x,y);
}

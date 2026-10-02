//type: fp
//options: 
# 0 "./analyzer/taint-assert.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/taint-assert.c"




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
# 6 "./analyzer/taint-assert.c" 2



extern void my_assert_fail (const char *expr, const char *file, int line)
  __attribute__ ((__noreturn__));




int
test_not_tainted_MY_ASSERT_1 (int n)
{
  do { if (!(n > 0)) my_assert_fail ("n > 0", "./analyzer/taint-assert.c", 18); } while (0);
  return n * n;
}

int __attribute__((tainted_args))
test_tainted_MY_ASSERT_1 (int n)
{
  do { if (!(n > 0)) my_assert_fail ("n > 0", "./analyzer/taint-assert.c", 25); } while (0);

  return n * n;
}







int
test_not_tainted_MY_ASSERT_2 (int n)
{
  do { if (!(n > 0)) __builtin_unreachable (); } while (0);
  return n * n;
}

int __attribute__((tainted_args))
test_tainted_MY_ASSERT_2 (int n)
{
  do { if (!(n > 0)) __builtin_unreachable (); } while (0);

  return n * n;
}







int
test_not_tainted_MY_ASSERT_3 (int n)
{
  do { } while (0);
  return n * n;
}

int __attribute__((tainted_args))
test_tainted_MY_ASSERT_3 (int n)
{
  do { } while (0);
  return n * n;
}




extern void do_something_benign ();




int
test_not_tainted_NOT_AN_ASSERT (int n)
{
  do { if (!(n > 0)) do_something_benign (); } while (0);
  return n * n;
}

int __attribute__((tainted_args))
test_tainted_NOT_AN_ASSERT (int n)
{
  do { if (!(n > 0)) do_something_benign (); } while (0);
  return n * n;
}




int __attribute__((tainted_args))
test_tainted_condition (int n)
{
  if (n > 0)
    return 1;
  else
    return -1;
}




int g;

void __attribute__((tainted_args))
test_compound_condition_in_assert_1 (int n)
{
  do { if (!((n * 2) < (g + 3))) my_assert_fail ("(n * 2) < (g + 3)", "./analyzer/taint-assert.c", 113); } while (0);
}

void __attribute__((tainted_args))
test_compound_condition_in_assert_2 (int x, int y)
{
  do { if (!(x < 100 && y < 100)) my_assert_fail ("x < 100 && y < 100", "./analyzer/taint-assert.c", 119); } while (0);
}

void __attribute__((tainted_args))
test_compound_condition_in_assert_3 (int x, int y)
{
  do { if (!(x < 100 || y < 100)) my_assert_fail ("x < 100 || y < 100", "./analyzer/taint-assert.c", 125); } while (0);
}

void __attribute__((tainted_args))
test_sanitized_expression_in_assert (int n)
{
  __analyzer_dump_state ("taint", n);
  if (n < 0 || n >= 100)
    return;
  __analyzer_dump_state ("taint", n);
  do { if (!(n < 200)) my_assert_fail ("n < 200", "./analyzer/taint-assert.c", 135); } while (0);
}

void __attribute__((tainted_args))
test_sanitization_then_ok_assertion (unsigned n)
{
  if (n >= 100)
    return;


  do { if (!(g > 42)) my_assert_fail ("g > 42", "./analyzer/taint-assert.c", 145); } while (0);
}

void __attribute__((tainted_args))
test_good_assert_then_bad_assert (unsigned n)
{

  do { if (!(g > 42)) my_assert_fail ("g > 42", "./analyzer/taint-assert.c", 152); } while (0);


  do { if (!(n < 100)) my_assert_fail ("n < 100", "./analyzer/taint-assert.c", 155); } while (0);
}

void __attribute__((tainted_args))
test_bad_assert_then_good_assert (unsigned n)
{
  do { if (!(n < 100)) my_assert_fail ("n < 100", "./analyzer/taint-assert.c", 161); } while (0);
  do { if (!(g > 42)) my_assert_fail ("g > 42", "./analyzer/taint-assert.c", 162); } while (0);
}




void __attribute__((tainted_args))
test_zero_MY_ASSERT_1 (unsigned n)
{
  if (n >= 100)
    do { if (!(0)) my_assert_fail ("0", "./analyzer/taint-assert.c", 172); } while (0);
}

void __attribute__((tainted_args))
test_nonzero_MY_ASSERT_1 (unsigned n)
{
  if (n >= 100)
    do { if (!(1)) my_assert_fail ("1", "./analyzer/taint-assert.c", 179); } while (0);
}

void __attribute__((tainted_args))
test_zero_MY_ASSERT_2 (unsigned n)
{
  if (n >= 100)
    do { if (!(0)) __builtin_unreachable (); } while (0);
}

void __attribute__((tainted_args))
test_nonzero_MY_ASSERT_2 (unsigned n)
{
  if (n >= 100)
    do { if (!(1)) __builtin_unreachable (); } while (0);
}




static int
__analyzer_valid_1 (int x)
{
  return x < 100;
}

void __attribute__((tainted_args))
test_assert_calling_valid_1 (int n)
{
  do { if (!(__analyzer_valid_1 (n))) my_assert_fail ("__analyzer_valid_1 (n)", "./analyzer/taint-assert.c", 208); } while (0);
}

static int
__analyzer_valid_2 (int x)
{
  return x < 100;
}

void __attribute__((tainted_args))
test_assert_calling_valid_2 (int n)
{
  do { if (!(__analyzer_valid_2 (n))) my_assert_fail ("__analyzer_valid_2 (n)", "./analyzer/taint-assert.c", 220); } while (0);
}

static int
__analyzer_valid_3 (int x, int y)
{
  if (x >= 100)
    return 0;
  if (y >= 100)
    return 0;
  return 1;
}

void __attribute__((tainted_args))
test_assert_calling_valid_3 (int a, int b)
{
  do { if (!(__analyzer_valid_3 (a, b))) my_assert_fail ("__analyzer_valid_3 (a, b)", "./analyzer/taint-assert.c", 236); } while (0);
}




int __attribute__((tainted_args))
test_switch_default (int n)
{
  switch (n)

    {
    case 0:
      return 5;
    case 1:
      return 22;
    case 2:
      return -1;
    default:

      __builtin_unreachable ();
    }
}

int __attribute__((tainted_args))
test_switch_unhandled_case (int n)
{
  switch (n)

    {
    case 0:
      return 5;
    case 1:
      return 22;
    case 2:
      return -1;
    }


  __builtin_unreachable ();
}

int __attribute__((tainted_args))
test_switch_bogus_case_MY_ASSERT_1 (int n)
{
  switch (n)
    {
    default:
    case 0:
      return 5;
    case 1:
      return 22;
    case 2:
      return -1;
    case 42:
      do { if (!(0)) my_assert_fail ("0", "./analyzer/taint-assert.c", 291); } while (0);
    }
}

int __attribute__((tainted_args))
test_switch_bogus_case_MY_ASSERT_2 (int n)
{
  switch (n)
    {
    default:
    case 0:
      return 5;
    case 1:
      return 22;
    case 2:
      return -1;
    case 42:
      do { if (!(0)) __builtin_unreachable (); } while (0);
    }
}

int __attribute__((tainted_args))
test_switch_bogus_case_unreachable (int n)
{
  switch (n)
    {
    default:
    case 0:
      return 5;
    case 1:
      return 22;
    case 2:
      return -1;
    case 42:

      __builtin_unreachable ();
    }
}




struct s
{
  int x;
  int y;
};

int __attribute__((tainted_args))
test_assert_struct (struct s *p)
{
  do { if (!(p->x < p->y)) my_assert_fail ("p->x < p->y", "./analyzer/taint-assert.c", 342); } while (0);
}

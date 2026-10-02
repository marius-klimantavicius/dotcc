//type: fp
//options: 
# 0 "./analyzer/feasibility-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/feasibility-1.c"
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
# 2 "./analyzer/feasibility-1.c" 2

void test_1 (void)
{
  __analyzer_dump_path ();
}

void test_2 (int flag)
{
  if (flag)
    __analyzer_dump_path ();
}

void test_3 (int flag)
{
  if (flag)
    if (!flag)
      __analyzer_dump_path ();
}

int global_for_test_4;
static void __attribute__((noinline)) called_by_test_4 () {}
void test_4 (void)
{


  global_for_test_4 = 0;
  global_for_test_4 = 1;

  called_by_test_4 ();
  if (global_for_test_4)
    __analyzer_dump_path ();
}



void test_5 (void)
{
  for (int i = 0; i < 1024; i++)
    {
    }
  __analyzer_dump_path ();
}





int test_6 (int a, int b)
{
  int problem = 0;
  if (a)
    problem = 1;
  if (b)
    {
      if (!problem)
 problem = 2;
      __analyzer_dump_path ();
    }
  return problem;
}






static void __attribute__((noinline))
called_by_test_6a (void *ptr)
{
  __builtin_free (ptr);
  __builtin_free (ptr);
}

int test_6a (int a, int b, void *ptr)
{
  int problem = 0;
  if (a)
    problem = 1;
  if (b)
    {
      if (!problem)
 problem = 2;
      called_by_test_6a (ptr);
    }
  return problem;
}




void test_7 (int n)
{
  int entered_loop = 0;
  int i;
  for (i = 0; i < n; i++)
    entered_loop = 1;
  if (entered_loop)
    __analyzer_dump_path ();
}

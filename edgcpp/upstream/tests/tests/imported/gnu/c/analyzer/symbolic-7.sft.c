//type: fp
//options: 
# 0 "./analyzer/symbolic-7.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/symbolic-7.c"
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
# 2 "./analyzer/symbolic-7.c" 2

extern void maybe_write (int *);

void test_1 (int i)
{

  int arr[2];
  arr[0] = 1066;
  arr[1] = 1776;


  __analyzer_eval (arr[0] == 1066);
  __analyzer_eval (arr[1] == 1776);


  __analyzer_describe (0, arr[i]);
  __analyzer_eval (arr[i] == 1776);
}

void test_2 (int i)
{

  int arr[2];
  maybe_write (arr);


  __analyzer_eval (arr[0] == 42);


  __analyzer_eval (arr[i] == 42);
}

void test_3_concrete_read (int i)
{

  int arr[2];


  __analyzer_eval (arr[0] == 42);
}

void test_3_symbolic_read (int i)
{

  int arr[2];


  __analyzer_eval (arr[i] == 42);
}

//type: fp
//options: 
# 0 "./analyzer/clobbers-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/clobbers-2.c"
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
# 2 "./analyzer/clobbers-2.c" 2

typedef long unsigned int size_t;
extern void bzero (void *s, size_t n);
extern void *memset(void *s, int c, size_t n);

void test_1 (void)
{
  char arr[16];
  bzero (arr, sizeof (arr));
  __analyzer_eval (arr[0] == 0);
  __analyzer_eval (arr[7] == 0);
  __analyzer_eval (arr[8] == 0);
  __analyzer_eval (arr[9] == 0);
  __analyzer_eval (arr[15] == 0);


  arr[8] = 42;
  __analyzer_eval (arr[0] == 0);
  __analyzer_eval (arr[7] == 0);
  __analyzer_eval (arr[8] == 0);
  __analyzer_eval (arr[8] == 42);
  __analyzer_eval (arr[9] == 0);
  __analyzer_eval (arr[15] == 0);
}

void test_2 (void)
{
  char arr[16];
  bzero (arr, sizeof (arr));
  __analyzer_eval (arr[0] == 0);
  __analyzer_eval (arr[1] == 0);
  __analyzer_eval (arr[15] == 0);


  arr[0] = 42;
  __analyzer_eval (arr[0] == 0);
  __analyzer_eval (arr[0] == 42);
  __analyzer_eval (arr[1] == 0);
  __analyzer_eval (arr[15] == 0);
}

void test_3 (void)
{
  char arr[16];
  bzero (arr, sizeof (arr));
  __analyzer_eval (arr[0] == 0);
  __analyzer_eval (arr[14] == 0);
  __analyzer_eval (arr[15] == 0);


  arr[15] = 42;
  __analyzer_eval (arr[0] == 0);
  __analyzer_eval (arr[14] == 0);
  __analyzer_eval (arr[15] == 0);
  __analyzer_eval (arr[15] == 42);
}

void test_4 (void)
{
  char arr[16];
  bzero (arr, sizeof (arr));
  __analyzer_eval (arr[0] == 0);
  __analyzer_eval (arr[15] == 0);


  memset (arr, 1, 16);
  __analyzer_eval (arr[0] == 0);
  __analyzer_eval (arr[15] == 0);
  __analyzer_eval (arr[0] == 1);
  __analyzer_eval (arr[15] == 1);
}

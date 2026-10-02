//type: fp
//options: 
# 0 "./analyzer/aliasing-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/aliasing-2.c"
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
# 2 "./analyzer/aliasing-2.c" 2

extern void escape (int *p);

int a;
void test (int *p, int x)
{
  int y;

  a = 17;
  x = 42;
  y = 13;

  __analyzer_eval (a == 17);
  __analyzer_eval (x == 42);
  __analyzer_eval (y == 13);

  escape (&x);
  __analyzer_eval (a == 17);
  __analyzer_eval (x == 42);
  __analyzer_eval (y == 13);

  __analyzer_eval (p == &a);
  __analyzer_eval (p == &x);
  __analyzer_eval (p == &y);

  *p = 73;

  __analyzer_eval (a == 17);
  __analyzer_eval (x == 42);
  __analyzer_eval (y == 13);
}

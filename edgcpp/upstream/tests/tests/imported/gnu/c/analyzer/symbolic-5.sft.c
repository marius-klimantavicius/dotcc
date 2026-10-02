//type: fp
//options: 
# 0 "./analyzer/symbolic-5.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/symbolic-5.c"
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
# 2 "./analyzer/symbolic-5.c" 2

int a[1024];
int b[1024];

extern void escape (void *ptr);

void test_1 (int *p)
{
  int c, d;
  escape (&c);
  a[16] = 42;
  b[16] = 17;
  c = 33;
  d = 44;
  __analyzer_eval (a[16] == 42);
  __analyzer_eval (b[16] == 17);
  __analyzer_eval (c == 33);
  __analyzer_eval (d == 44);


  *p = 100;

  __analyzer_eval (a[16] == 42);
  __analyzer_eval (b[16] == 17);

  __analyzer_eval (c == 33);
  __analyzer_eval (d == 44);
}

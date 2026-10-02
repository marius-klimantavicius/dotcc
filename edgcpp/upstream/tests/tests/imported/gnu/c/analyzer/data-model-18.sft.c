//type: fp
//options: 
# 0 "./analyzer/data-model-18.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/data-model-18.c"
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
# 2 "./analyzer/data-model-18.c" 2

void test (int *p, int i, int j)
{
  p[3] = 42;
  __analyzer_eval (p[3] == 42);
  __analyzer_eval (*(p + 3) == 42);
  __analyzer_eval (p[i] == 42);
  __analyzer_eval (p[j] == 42);



  p[i] = 17;



  __analyzer_eval (p[3] == 42);
  __analyzer_eval (p[i] == 17);
  __analyzer_eval (p[j] == 17);
}

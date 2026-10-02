//type: fp
//options: 
# 0 "./analyzer/pr97568.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr97568.c"
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
# 2 "./analyzer/pr97568.c" 2



extern int *const p1;

int *const p2;

int v3;
extern int *const p3 = &v3;

int v4;
int *const p4 = &v4;

int main (void)
{
  __analyzer_describe (0, p1);
  __analyzer_eval (p1 == ((void *)0));

  __analyzer_eval (p2 == ((void *)0));

  __analyzer_describe (0, p3);
  __analyzer_eval (p3 == ((void *)0));

  __analyzer_describe (0, p4);
  __analyzer_eval (p4 == ((void *)0));

  return p1[0];
}

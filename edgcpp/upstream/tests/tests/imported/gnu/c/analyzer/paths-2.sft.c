//type: fp
//options: 
# 0 "./analyzer/paths-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/paths-2.c"
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
# 2 "./analyzer/paths-2.c" 2

int test (int a)
{
  if (a != 42 && a != 113) {
    return (-2);
  }

  __analyzer_dump_exploded_nodes (0);

  return 0;
}

int test_2 (int a)
{
  if (a != 42 && a != 113 && a != 666) {
    return (-2);
  }

  __analyzer_dump_exploded_nodes (0);

  return 0;
}

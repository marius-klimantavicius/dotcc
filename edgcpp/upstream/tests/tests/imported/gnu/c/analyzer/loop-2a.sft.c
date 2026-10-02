//type: fp
//options: 
# 0 "./analyzer/loop-2a.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/loop-2a.c"

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
# 3 "./analyzer/loop-2a.c" 2

union u
{
  int i;
};

void test(void)
{
  union u u;

  __analyzer_dump_exploded_nodes (0);


  for (u.i=0; u.i<256; u.i++) {
    __analyzer_eval (u.i < 256);

    __analyzer_dump_exploded_nodes (0);






    __analyzer_eval (u.i >= 0);
  }

  __analyzer_eval (u.i >= 256);

  __analyzer_eval (u.i == 256);



  __analyzer_dump_exploded_nodes (0);
}

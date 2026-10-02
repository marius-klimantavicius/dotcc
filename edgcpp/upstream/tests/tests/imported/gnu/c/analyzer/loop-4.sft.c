//type: fp
//options: 
# 0 "./analyzer/loop-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/loop-4.c"


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
# 4 "./analyzer/loop-4.c" 2

void test(void)
{
  int i, j, k;

  __analyzer_dump_exploded_nodes (0);

  for (i=0; i<256; i++) {

    __analyzer_eval (i >= 0);

    __analyzer_eval (i < 256);

    for (j=0; j<256; j++) {

      __analyzer_eval (j >= 0);


      __analyzer_eval (j < 256);



      __analyzer_dump_exploded_nodes (0);

      for (k=0; k<256; k++) {

 __analyzer_eval (k >= 0);


 __analyzer_eval (k < 256);


 __analyzer_dump_exploded_nodes (0);
      }
    }
  }

  __analyzer_dump_exploded_nodes (0);
}

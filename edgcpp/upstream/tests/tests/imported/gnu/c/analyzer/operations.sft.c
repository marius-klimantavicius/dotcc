//type: fp
//options: 
# 0 "./analyzer/operations.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/operations.c"
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
# 2 "./analyzer/operations.c" 2

void test (int i, int j)
{
  int k, m;

  if (i > 42) {
    __analyzer_eval (i > 42);

    i += 3;

    __analyzer_eval (i > 45);



    i -= 1;

    __analyzer_eval (i > 44);



    i = 3 * i;

    __analyzer_eval (i > 132);



    i /= 2;

    __analyzer_eval (i > 66);




    k = i + j;
    __analyzer_eval (k == 0);


    m = i + 1;
    __analyzer_eval (m > 67);


  }
}

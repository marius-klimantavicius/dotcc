//type: fp
//options: 
# 0 "./analyzer/params.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/params.c"
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
# 2 "./analyzer/params.c" 2

static int __analyzer_called_function(int j)
{
  int k;

  __analyzer_eval (j > 4);

  k = j - 1;

  __analyzer_eval (k > 3);



  return k;
}

void test(int i)
{
  __analyzer_eval (i > 4);

  if (i > 4) {

    __analyzer_eval (i > 4);

    i = __analyzer_called_function(i);

    __analyzer_eval (i > 3);


  }

  __analyzer_eval (i > 3);
}

//type: fp
//options: 
# 0 "./analyzer/feasibility-pr107948.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/feasibility-pr107948.c"
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
# 2 "./analyzer/feasibility-pr107948.c" 2

void foo(int width) {
  int i = 0;
  int base;
  if (width > 0){
    __analyzer_eval(i == 0);
    __analyzer_eval(width > 0);
    __analyzer_eval(width - i > 0);
    __analyzer_eval(i - width <= 0);
    if (i - width <= 0) {
      base = 512;
    }
    else {
      __analyzer_dump_path ();
    }
    base+=1;
  }
}

void test_ge_zero (int x)
{
  if (x >= 0)
    {
      __analyzer_eval(x >= 0);
      __analyzer_eval(x > 0);
      __analyzer_eval(x <= 0);
      __analyzer_eval(x < 0);
      __analyzer_eval(-x <= 0);
      __analyzer_eval(-x < 0);
      __analyzer_eval(-x >= 0);
      __analyzer_eval(-x > 0);
    }
}

void test_gt_zero (int x)
{
  if (x > 0)
    {
      __analyzer_eval(x >= 0);
      __analyzer_eval(x > 0);
      __analyzer_eval(x <= 0);
      __analyzer_eval(x < 0);
      __analyzer_eval(-x <= 0);
      __analyzer_eval(-x < 0);
      __analyzer_eval(-x >= 0);
      __analyzer_eval(-x > 0);
    }
}

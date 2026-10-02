//type: fp
//options: 
# 0 "./analyzer/symbolic-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/symbolic-2.c"
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
# 2 "./analyzer/symbolic-2.c" 2

struct foo
{
  int ival;
  int iarr[10];
};

void test_1 (int i, int j)
{
  struct foo fooarr[4];
  fooarr[1].ival = 42;
  fooarr[1].iarr[3] = 27;
  fooarr[2].iarr[1] = 17;
  __analyzer_eval (fooarr[1].ival == 42);
  __analyzer_eval (fooarr[1].iarr[3] == 27);
  __analyzer_eval (fooarr[2].iarr[1] == 17);


  fooarr[2].iarr[i] = j;
  __analyzer_eval (fooarr[2].iarr[i] == j);




  __analyzer_eval (fooarr[1].ival == 42);
  __analyzer_eval (fooarr[1].iarr[3] == 27);
  __analyzer_eval (fooarr[2].iarr[1] == 17);


  __analyzer_eval (fooarr[2].iarr[10] == 17);
}

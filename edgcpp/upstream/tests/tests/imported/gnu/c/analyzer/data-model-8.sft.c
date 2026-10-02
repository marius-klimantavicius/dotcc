//type: fp
//options: 
# 0 "./analyzer/data-model-8.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/data-model-8.c"
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
# 2 "./analyzer/data-model-8.c" 2

struct base
{
  int i;
};

struct sub
{
  struct base b;
  int j;
};

void test (void)
{
  struct sub s;
  s.b.i = 3;
  s.j = 4;
  __analyzer_eval (s.b.i == 3);
  __analyzer_eval (s.j == 4);

  struct base *bp = (struct base *)&s;

  __analyzer_eval (bp->i == 3);
}

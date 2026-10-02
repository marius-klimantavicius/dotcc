//type: fp
//options: 
# 0 "./analyzer/pr96650-2-trans.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr96650-2-trans.c"


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
# 4 "./analyzer/pr96650-2-trans.c" 2

int foo (void);



void test_1 (int co, int y)
{
  if (4 < co)
    if (co < y)
      if (y == 0)
 __analyzer_dump_path ();
}



void test_2 (int co, int y, int z)
{
  if (4 < co)
    if (co < y)
      if (y == 0)
 {
   while (foo ())
     {
     }
   __analyzer_dump_path ();
 }
}

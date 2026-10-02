//type: fp
//options: 
# 0 "./analyzer/pr94362-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr94362-2.c"



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
# 5 "./analyzer/pr94362-2.c" 2

void test_1 (int idx)
{
  if (idx > 0)
    if (idx - 1 < 0)
      __analyzer_dump_path ();
}

static int called_by_test_1a (int idx)
{
  return idx - 1;
}

void test_1a (int idx)
{
  if (idx > 0)
    if (called_by_test_1a (idx) < 0)
      __analyzer_dump_path ();
}

void test_2 (int idx)
{
  if (idx + 1 > 0)
    if (idx < 0)
      __analyzer_dump_path ();
}

static int called_by_test_2a (int idx)
{
  return idx + 1;
}

void test_2a (int idx)
{
  if (called_by_test_2a (idx) > 0)
    if (idx < 0)
      __analyzer_dump_path ();
}

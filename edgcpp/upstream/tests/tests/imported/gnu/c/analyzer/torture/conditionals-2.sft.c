//type: fp
//options: 
# 0 "./analyzer/torture/conditionals-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/torture/conditionals-2.c"


# 1 "./analyzer/torture/../analyzer-decls.h" 1
# 20 "./analyzer/torture/../analyzer-decls.h"
extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 44 "./analyzer/torture/../analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);




extern long unsigned int __analyzer_get_strlen (const char *ptr);
# 4 "./analyzer/torture/conditionals-2.c" 2



static void __attribute__((noinline))
__analyzer_test_1_callee (void *p, void *q)
{
  __analyzer_dump_exploded_nodes (0);

  __analyzer_eval (p == 0);
  __analyzer_eval (p != 0);

  __analyzer_eval (q == 0);
  __analyzer_eval (q != 0);
}

void test_1 (void *p, void *q)
{
  if (p == 0 || q == 0)
    return;

  __analyzer_test_1_callee (p, q);
}

static void __attribute__((noinline))
__analyzer_test_2_callee (void *p, void *q)
{
  __analyzer_dump_exploded_nodes (0);

  __analyzer_eval (p == 0);
  __analyzer_eval (p != 0);

  __analyzer_eval (q == 0);
  __analyzer_eval (q != 0);
}

void test_2 (void *p, void *q)
{
  if (p != 0 && q != 0)
    __analyzer_test_2_callee (p, q);
}

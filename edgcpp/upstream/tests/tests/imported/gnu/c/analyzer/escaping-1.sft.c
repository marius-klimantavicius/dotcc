//type: fp
//options: 
# 0 "./analyzer/escaping-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/escaping-1.c"
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
# 2 "./analyzer/escaping-1.c" 2



extern void unknown_fn (void *);

static int only_used_by_test_1;

static void test_1 (void)
{
  int local_1, local_2;
  __analyzer_dump_escaped ();

  unknown_fn (((void *)0));
  __analyzer_dump_escaped ();

  unknown_fn (&local_1);
  __analyzer_dump_escaped ();


  unknown_fn (&local_1);
  __analyzer_dump_escaped ();


  unknown_fn (&only_used_by_test_1);
  __analyzer_dump_escaped ();
}

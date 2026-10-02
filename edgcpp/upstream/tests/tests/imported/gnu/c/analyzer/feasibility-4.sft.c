//type: fp
//options: 
# 0 "./analyzer/feasibility-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/feasibility-4.c"
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
# 2 "./analyzer/feasibility-4.c" 2

extern int rand (void);

void test_1 (void)
{
  int ret = 0;
  while (ret != 42)
    ret = rand() % 1000;

  if (ret != 42)
    __analyzer_dump_path ();
}

static void empty_local_fn (void) {}
extern void external_fn (void);

void test_2 (void)
{
  void (*callback) () = empty_local_fn;
  int ret = 0;
  while (ret != 42)
    ret = rand() % 1000;

  (*callback) ();

  if (ret != 42)
    __analyzer_dump_path ();
}

void test_3 (void)
{
  void (*callback) () = external_fn;
  int ret = 0;
  while (ret != 42)
    ret = rand() % 1000;

  (*callback) ();

  if (ret != 42)
    __analyzer_dump_path ();
}

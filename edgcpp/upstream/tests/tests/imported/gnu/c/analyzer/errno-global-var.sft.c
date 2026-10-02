//type: fp
//options: 
# 0 "./analyzer/errno-global-var.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/errno-global-var.c"
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
# 2 "./analyzer/errno-global-var.c" 2



extern int errno;

extern void external_fn (void);

int test_reading_errno (void)
{
  return errno;
}

void test_setting_errno (int val)
{
  errno = val;
}

void test_storing_to_errno (int val)
{
  __analyzer_eval (errno == val);
  errno = val;
  __analyzer_eval (errno == val);
  external_fn ();
  __analyzer_eval (errno == val);
}

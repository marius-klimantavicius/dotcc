//type: fp
//options: 
# 0 "./analyzer/abs-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/abs-1.c"
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
# 2 "./analyzer/abs-1.c" 2

extern long int labs (long int x)
  __attribute__ ((__nothrow__ , __leaf__))
  __attribute__ ((__const__));

long int test_1 (long int x)
{
  return labs (x);
}

static long __attribute__((noinline))
hide_long (long x)
{
  return x;
}

long int test_2 (long int x)
{
  __analyzer_eval (labs (hide_long (42)) == 42);
  __analyzer_eval (labs (hide_long (-17)) == 17);
}

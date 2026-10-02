//type: fp
//options: 
# 0 "./analyzer/pr97074.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr97074.c"
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
# 2 "./analyzer/pr97074.c" 2


void *x, *y;

void test_1 (int flag)
{
  void *p = __builtin_malloc (1024);
  if (flag)
    x = p;
  else
    y = p;
}

struct s2
{
  void *f1;
  void *f2;
};

struct s2 test_2 (int flag)
{
  struct s2 r;
  r.f1 = ((void *)0);
  r.f2 = ((void *)0);
  void *p = __builtin_malloc (1024);
  if (flag)
    r.f1 = p;
  else
    r.f2 = p;
  return r;
}

//type: fp
//options: 
# 0 "./analyzer/data-model-23.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/data-model-23.c"
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
# 2 "./analyzer/data-model-23.c" 2



void * __attribute__((noinline))
hide (void *ptr)
{
  return ptr;
}

void test_1 (void)
{
  int a;
  __analyzer_eval (hide (&a) == ((void *)0));
  __analyzer_eval (hide (&a) + 1 != ((void *)0));
  __analyzer_eval (hide (&a) + 1 == ((void *)0));
  __analyzer_eval (hide (&a) - 1 != ((void *)0));
  __analyzer_eval (hide (&a) - 1 == ((void *)0));
}

void test_2 (void)
{
  __analyzer_eval (hide (((void *)0)) == ((void *)0));
  __analyzer_eval (hide (((void *)0)) - 1 == ((void *)0));
  __analyzer_eval (hide (((void *)0)) + 1 == ((void *)0));
}

void test_3 (void *p)
{
  if (!p)
    return;
  __analyzer_eval (hide (p) == ((void *)0));
  __analyzer_eval (hide (p) + 1 != ((void *)0));
  __analyzer_eval (hide (p) + 1 == ((void *)0));
  __analyzer_eval (hide (p) - 1 != ((void *)0));
  __analyzer_eval (hide (p) - 1 == ((void *)0));
}

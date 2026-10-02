//type: fp
//options: 
# 0 "./analyzer/pr101962.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr101962.c"
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
# 2 "./analyzer/pr101962.c" 2






static int * __attribute__((noinline))
maybe_inc_int_ptr (int *ptr)
{
  if (!ptr)
    return ((void *)0);
  return ++ptr;
}

int
test_1 (void)
{
  int stack;
  int *a = &stack;
  a = maybe_inc_int_ptr (a);
  a = maybe_inc_int_ptr (a);
  __analyzer_eval (a == ((void *)0));
  __analyzer_eval (a != ((void *)0));
  return *a;



}

static const char * __attribute__((noinline))
maybe_inc_char_ptr (const char *ptr)
{
  if (!ptr)
    return ((void *)0);
  return ++ptr;
}

char
test_s (void)
{
  const char *msg = "hello world";
  const char *a = msg;
  __analyzer_eval (*a == 'h');
  a = maybe_inc_char_ptr (a);
  __analyzer_eval (*a == 'e');
  a = maybe_inc_char_ptr (a);
  __analyzer_eval (*a == 'l');
  a = maybe_inc_char_ptr (a);
  __analyzer_eval (*a == 'l');
  a = maybe_inc_char_ptr (a);
  __analyzer_eval (*a == 'o');
}

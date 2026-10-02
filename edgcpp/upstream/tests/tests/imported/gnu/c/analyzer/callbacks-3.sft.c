//type: fp
//options: 
# 0 "./analyzer/callbacks-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/callbacks-3.c"
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
# 2 "./analyzer/callbacks-3.c" 2

typedef long unsigned int size_t;
typedef int (*__compar_fn_t)(const void *, const void *);
extern void qsort(void *__base, size_t __nmemb, size_t __size,
    __compar_fn_t __compar)
  __attribute__((__nonnull__(1, 4)));

static int
test_1_callback (const void *p1, const void *p2)
{
  __analyzer_dump_path ();
  return 0;
}

void test_1_caller (int *arr, size_t n)
{
  qsort (arr, n, sizeof (int), test_1_callback);
}

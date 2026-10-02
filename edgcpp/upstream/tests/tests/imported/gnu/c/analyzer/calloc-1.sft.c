//type: fp
//options: 
# 0 "./analyzer/calloc-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/calloc-1.c"
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
# 2 "./analyzer/calloc-1.c" 2

typedef long unsigned int size_t;



extern void *calloc (size_t __nmemb, size_t __size)
  __attribute__ ((__nothrow__ , __leaf__))
  __attribute__ ((__malloc__))
  __attribute__ ((__alloc_size__ (1, 2))) ;

char *test_1 (size_t sz)
{
  char *p;

  p = calloc (1, 3);
  if (!p)
    return ((void *)0);

  __analyzer_dump_capacity (p);

  __analyzer_eval (p[0] == 0);
  __analyzer_eval (p[1] == 0);
  __analyzer_eval (p[2] == 0);

  return p;
}

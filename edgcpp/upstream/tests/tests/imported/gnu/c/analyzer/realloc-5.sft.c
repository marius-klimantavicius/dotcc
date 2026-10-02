//type: fp
//options: 
# 0 "./analyzer/realloc-5.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/realloc-5.c"


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
# 4 "./analyzer/realloc-5.c" 2

typedef long unsigned int size_t;



extern void *malloc (size_t __size)
  __attribute__ ((__nothrow__ , __leaf__))
  __attribute__ ((__malloc__))
  __attribute__ ((__alloc_size__ (1)));
extern void *realloc (void *__ptr, size_t __size)
  __attribute__ ((__nothrow__ , __leaf__))
  __attribute__ ((__warn_unused_result__))
  __attribute__ ((__alloc_size__ (2)));
extern void free (void *__ptr)
  __attribute__ ((__nothrow__ , __leaf__));
extern void *memset (void *__ptr, int __value, size_t __size);



void test_1 ()
{
  char *p = malloc (16);
  if (!p)
    return;
  memset (p, 1, 16);

  char *q = realloc (p, 8);
  if (!q)
    {
      free (p);
      return;
    }
  else if (p != q)
    {
      __analyzer_dump_capacity (q);
      __analyzer_eval (q[8] == 1);




    }

  free (q);
}

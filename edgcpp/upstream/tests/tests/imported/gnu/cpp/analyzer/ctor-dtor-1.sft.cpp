//type: fp
//options: 
# 0 "./analyzer/ctor-dtor-1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/ctor-dtor-1.C"
# 1 "./analyzer/../../gcc.dg/analyzer/analyzer-decls.h" 1
# 20 "./analyzer/../../gcc.dg/analyzer/analyzer-decls.h"
extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 44 "./analyzer/../../gcc.dg/analyzer/analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);




extern long unsigned int __analyzer_get_strlen (const char *ptr);
# 2 "./analyzer/ctor-dtor-1.C" 2

int foo_count;

struct foo
{
  foo () __attribute__((noinline))
  {
    foo_count++;
  }
  ~foo () __attribute__((noinline))
  {
    foo_count--;
  }
};

int main ()
{
  __analyzer_eval (foo_count == 0);
  {
    foo f;
    __analyzer_eval (foo_count == 1);
  }
  __analyzer_eval (foo_count == 0);
  return 0;
}

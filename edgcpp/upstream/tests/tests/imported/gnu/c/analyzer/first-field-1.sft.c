//type: fp
//options: 
# 0 "./analyzer/first-field-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/first-field-1.c"
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
# 2 "./analyzer/first-field-1.c" 2

typedef struct base_obj
{
  int m_first;
  int m_second;
} base_obj;

typedef struct sub_obj
{
  base_obj base;
} sub_obj;

void test (sub_obj *sub)
{
  sub->base.m_first = 1;
  sub->base.m_second = 2;
  __analyzer_eval (sub->base.m_first == 1);
  __analyzer_eval (sub->base.m_second == 2);

  base_obj *base = (struct base_obj *)sub;
  __analyzer_eval (base->m_first == 1);
  __analyzer_eval (base->m_second == 2);
}

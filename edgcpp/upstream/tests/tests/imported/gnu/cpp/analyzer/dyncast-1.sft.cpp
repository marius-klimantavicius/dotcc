//type: fp
//options: 
# 0 "./analyzer/dyncast-1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/dyncast-1.C"
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
# 2 "./analyzer/dyncast-1.C" 2

struct base
{
  virtual ~base () {}
};
struct sub : public base
{
  int m_field;
};

int
test_1 (base *p)
{
  if (sub *q = dynamic_cast <sub*> (p))
    {
      __analyzer_dump_path ();
      return q->m_field;
    }
  return 0;
}

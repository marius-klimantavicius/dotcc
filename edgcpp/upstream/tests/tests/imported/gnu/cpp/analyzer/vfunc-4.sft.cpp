//type: fp
//options: 
# 0 "./analyzer/vfunc-4.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/vfunc-4.C"
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
# 2 "./analyzer/vfunc-4.C" 2

struct A
{
  int m_data;
  virtual char foo ()
  {
    return 'A';
  }
};

struct B: public A
{
  int m_data_b;
  char foo ()
  {
    return 'B';
  }
};

void test()
{
  A a, *a_ptr = &a;
  B b;
  __analyzer_eval (a_ptr->foo () == 'A');
  a_ptr = &b;
  __analyzer_eval (a_ptr->foo () == 'B');
}

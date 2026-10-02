//type: fp
//options: 
# 0 "./analyzer/refcounting-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/refcounting-1.c"
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
# 2 "./analyzer/refcounting-1.c" 2

typedef struct obj {
  int ob_refcnt;
} PyObject;

extern void Py_Dealloc (PyObject *op);
# 22 "./analyzer/refcounting-1.c"
void test_1 (PyObject *obj)
{
  int orig_refcnt = obj->ob_refcnt;
  do { ((PyObject*)(obj))->ob_refcnt++; } while (0);
  do { ((PyObject*)(obj))->ob_refcnt++; } while (0);
  do { if (--((PyObject*)(obj))->ob_refcnt == 0) { } } while (0);
  do { ((PyObject*)(obj))->ob_refcnt++; } while (0);
  __analyzer_eval (obj->ob_refcnt == orig_refcnt + 2);
}

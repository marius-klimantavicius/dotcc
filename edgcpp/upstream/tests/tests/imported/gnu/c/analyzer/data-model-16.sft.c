//type: fp
//options: 
# 0 "./analyzer/data-model-16.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/data-model-16.c"




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
# 6 "./analyzer/data-model-16.c" 2

extern void foo (void);

void *x, *y, *z;

void test (void)
{
 label0:
  foo ();
 label1:
  foo ();
 label2:
  foo ();

  x = &&label0;
  y = &&label1;
  z = &&label2;

  __analyzer_eval (x == x);
  __analyzer_eval (x == y);
}

void test_2 (int i)
{
  static void *array[] = { &&label0, &&label1, &&label2 };
  goto *array[i];

 label0:
  foo ();
 label1:
  foo ();
 label2:
  foo ();
}

void test_3 (int i)
{
  static const int array[] = { &&label0 - &&label0,
          &&label1 - &&label0,
          &&label2 - &&label0 };
  goto *(&&label0 + array[i]);

 label0:
  foo ();
 label1:
  foo ();
 label2:
  foo ();
}

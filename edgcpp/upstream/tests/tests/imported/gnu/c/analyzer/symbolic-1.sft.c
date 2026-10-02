//type: fp
//options: 
# 0 "./analyzer/symbolic-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/symbolic-1.c"


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
# 4 "./analyzer/symbolic-1.c" 2



void test_1 (char a, char b, char c, char d, char e, char f,
      int i, int j)
{
  char arr[1024];
  arr[2] = a;
  arr[3] = b;

  __analyzer_eval (arr[2] == a);
  __analyzer_eval (arr[3] == b);
  __analyzer_eval (arr[4]);



  arr[3] = c;
  __analyzer_eval (arr[2] == a);
  __analyzer_eval (arr[3] == c);
  __analyzer_eval (arr[3] == b);
  __analyzer_eval (arr[4]);



  arr[i] = d;
  __analyzer_eval (arr[i] == d);
  __analyzer_eval (arr[2] == a);
  __analyzer_eval (arr[3] == c);
  __analyzer_eval (arr[4]);


  arr[j] = e;
  __analyzer_eval (arr[j] == e);
  __analyzer_eval (arr[i] == d);
  __analyzer_eval (arr[4]);


  arr[3] = f;
  __analyzer_eval (arr[3] == f);
  __analyzer_eval (arr[j] == e);
  __analyzer_eval (arr[4]);
}

//type: fp
//options: 
# 0 "./analyzer/compound-assignment-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/compound-assignment-4.c"
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
# 2 "./analyzer/compound-assignment-4.c" 2

struct coord
{
  int x;
  int y;
};

void test_1 (void)
{
  struct coord arr[16];

  arr[2].y = 4;
  arr[3].x = 5;
  arr[3].y = 6;
  arr[4].x = 7;
  arr[6].y = 8;
  arr[8].x = 9;

  arr[7] = arr[3];

  __analyzer_eval (arr[7].x == 5);
  __analyzer_eval (arr[7].y == 6);


  __analyzer_eval (arr[6].y == 8);
  __analyzer_eval (arr[8].x == 9);
}

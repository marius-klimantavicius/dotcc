//type: fp
//options: 
# 0 "./analyzer/compound-assignment-5.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/compound-assignment-5.c"
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
# 2 "./analyzer/compound-assignment-5.c" 2

struct coord
{
  int x;
  int y;
};



void test_1 (void)
{
  struct coord arr_a[16];
  struct coord arr_b[16];
  arr_a[3].x = 5;
  arr_a[3].y = 6;

  arr_b[7] = arr_a[3];

  __analyzer_eval (arr_b[7].x == 5);
  __analyzer_eval (arr_b[7].y == 6);
}



struct coord glob_arr[16];

void test_2 (void)
{
  struct coord arr[16];
  arr[3].x = 5;
  arr[3].y = 6;

  glob_arr[7] = arr[3];

  __analyzer_eval (glob_arr[7].x == 5);
  __analyzer_eval (glob_arr[7].y == 6);
}



struct coord glob_arr[16];

void test_3 (void)
{
  struct coord arr[16];
  arr[3].y = 6;

  glob_arr[7] = arr[3];

  __analyzer_eval (glob_arr[7].x);

  __analyzer_eval (glob_arr[7].y == 6);
}



struct coord glob_arr[16];

void test_4 (int i)
{
  struct coord arr_a[16];
  struct coord arr_b[16];
  arr_a[i].x = 5;
  arr_a[i].y = 6;
  __analyzer_eval (arr_a[i].x == 5);

  __analyzer_eval (arr_a[i].y == 6);

  arr_b[i] = arr_a[i];

  __analyzer_eval (arr_b[i].x == 5);

  __analyzer_eval (arr_b[i].y == 6);

}



struct coord glob_arr[16];

void test_5a (int i, int j)
{
  struct coord arr[16];
  arr[i].x = 5;
  arr[i].y = 6;

  arr[j] = arr[i];

  __analyzer_eval (arr[j].x == 5);

  __analyzer_eval (arr[j].y == 6);

}



struct coord glob_arr[16];

void test_5b (int i)
{
  struct coord arr[16];
  arr[i].x = 5;
  arr[i].y = 6;

  arr[3] = arr[i];

  __analyzer_eval (arr[3].x == 5);

  __analyzer_eval (arr[3].y == 6);

}



struct coord glob_arr[16];

void test_5c (int i)
{
  struct coord arr[16];
  arr[3].x = 5;
  arr[3].y = 6;

  arr[i] = arr[3];

  __analyzer_eval (arr[i].x == 5);

  __analyzer_eval (arr[i].y == 6);

}




void test_6 (void)
{
  struct coord arr[16];
  arr[7] = glob_arr[3];

  __analyzer_eval (arr[7].x == 5);
  __analyzer_eval (arr[7].y == 6);
}

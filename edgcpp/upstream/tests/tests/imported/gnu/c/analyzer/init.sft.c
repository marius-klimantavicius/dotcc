//type: fp
//options: 
# 0 "./analyzer/init.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/init.c"







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
# 9 "./analyzer/init.c" 2

struct coord
{
  int x;
  int y;
};

struct tri
{
  struct coord v[3];
};

union iap
{
  int i;
  void *p;
};

void test_1 (void)
{
  struct coord c = {3, 4};
  __analyzer_eval (c.x == 3);
  __analyzer_eval (c.y == 4);
}

void test_2 (void)
{
  struct coord c = {3};
  __analyzer_eval (c.x == 3);
  __analyzer_eval (c.y == 0);
}

void test_3 (void)
{
  struct coord c = {};
  __analyzer_eval (c.x == 0);
  __analyzer_eval (c.y == 0);
}

void test_4 (void)
{
  int c[2] = {3, 4};
  __analyzer_eval (c[0] == 3);
  __analyzer_eval (c[1] == 4);
}

void test_5 (void)
{
  int c[2] = {3};
  __analyzer_eval (c[0] == 3);
  __analyzer_eval (c[1] == 0);
}

void test_6 (void)
{
  int c[2] = {};
  __analyzer_eval (c[0] == 0);
  __analyzer_eval (c[1] == 0);
}

void test_7 (void)
{
  struct coord c[2] = {{3, 4}, {5, 6}};
  __analyzer_eval (c[0].x == 3);
  __analyzer_eval (c[0].y == 4);
  __analyzer_eval (c[1].x == 5);
  __analyzer_eval (c[1].y == 6);
}

void test_8 (void)
{
  struct coord c[2] = {{3}, {5}};
  __analyzer_eval (c[0].x == 3);
  __analyzer_eval (c[0].y == 0);
  __analyzer_eval (c[1].x == 5);
  __analyzer_eval (c[1].y == 0);
}

void test_9 (void)
{
  struct coord c[2] = {{}, {}};
  __analyzer_eval (c[0].x == 0);
  __analyzer_eval (c[0].y == 0);
  __analyzer_eval (c[1].x == 0);
  __analyzer_eval (c[1].y == 0);
}

void test_10 (void)
{
  struct coord c[2] = {{.y = 4, .x = 3}, {5, 6}};
  __analyzer_eval (c[0].x == 3);
  __analyzer_eval (c[0].y == 4);
  __analyzer_eval (c[1].x == 5);
  __analyzer_eval (c[1].y == 6);
}

void test_11 (void)
{
  struct coord c[2] = {{.y = 4}, {5, 6}};
  __analyzer_eval (c[0].x == 0);
  __analyzer_eval (c[0].y == 4);
  __analyzer_eval (c[1].x == 5);
  __analyzer_eval (c[1].y == 6);
}

void test_12 (void)
{
  struct tri t = {};
  __analyzer_eval (t.v[0].x == 0);
  __analyzer_eval (t.v[2].y == 0);
}

void test_13 (void)
{
  struct tri t = {3, 4, 5, 6, 7, 8};
  __analyzer_eval (t.v[0].x == 3);
  __analyzer_eval (t.v[0].y == 4);
  __analyzer_eval (t.v[1].x == 5);
  __analyzer_eval (t.v[1].y == 6);
  __analyzer_eval (t.v[2].x == 7);
  __analyzer_eval (t.v[2].y == 8);
}

void test_14 (void)
{
  union iap u = {};
  __analyzer_eval (u.i == 0);
}

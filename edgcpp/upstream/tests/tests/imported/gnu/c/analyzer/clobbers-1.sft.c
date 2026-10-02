//type: fp
//options: 
# 0 "./analyzer/clobbers-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/clobbers-1.c"
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
# 2 "./analyzer/clobbers-1.c" 2

struct foo
{
  int i;
  int j;
};

struct coord
{
  int x;
  int y;
  int z;
};

struct foo g;

void test_1 (void)
{
  g.i = 42;
  if (g.j)
    __analyzer_eval (g.j);
  else
    __analyzer_eval (g.j);
  __analyzer_dump_exploded_nodes (0);
}

void test_2 (struct foo f)
{
  f.i = 42;
  if (f.j)
    __analyzer_eval (f.j);
  else
    __analyzer_eval (f.j);
  __analyzer_dump_exploded_nodes (0);
}

void test_3 (struct foo *p)
{
  struct foo f = *p;
  f.i = 42;
  if (f.j)
    __analyzer_eval (f.j);
  else
    __analyzer_eval (f.j);
  __analyzer_dump_exploded_nodes (0);
}

void test_4 (struct coord *p)
{
  struct coord f = *p;
  f.x = 42;
  __analyzer_eval (f.y == p->y);
  __analyzer_eval (f.z == p->z);
}

struct s5
{
  char arr[8];
};

void test_5 (struct s5 *p)
{
  struct s5 f = *p;
  f.arr[3] = 42;
  __analyzer_eval (f.arr[0] == p->arr[0]);
  __analyzer_eval (f.arr[1] == p->arr[1]);
  __analyzer_eval (f.arr[2] == p->arr[2]);
  __analyzer_eval (f.arr[3] == 42);
  __analyzer_eval (f.arr[4] == p->arr[4]);
  __analyzer_eval (f.arr[5] == p->arr[5]);
  __analyzer_eval (f.arr[6] == p->arr[6]);
  __analyzer_eval (f.arr[7] == p->arr[7]);
}

struct s6
{
  int before;
  struct foo arr[4];
  int after;
};

void test_6 (struct s6 *p, struct foo *q)
{
  struct s6 f = *p;
  f.arr[1] = *q;
  __analyzer_eval (f.before == p->before);
  __analyzer_eval (f.arr[0].i == p->arr[0].i);
  __analyzer_eval (f.arr[0].j == p->arr[0].j);
  __analyzer_eval (f.arr[1].i == q->i);
  __analyzer_eval (f.arr[1].j == q->j);
  __analyzer_eval (f.arr[2].i == p->arr[2].i);
  __analyzer_eval (f.arr[2].j == p->arr[2].j);
  __analyzer_eval (f.arr[3].i == p->arr[3].i);
  __analyzer_eval (f.arr[3].j == p->arr[3].j);
  __analyzer_eval (f.after == p->after);
}

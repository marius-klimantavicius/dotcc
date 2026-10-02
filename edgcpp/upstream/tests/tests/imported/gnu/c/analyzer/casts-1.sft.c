//type: fp
//options: 
# 0 "./analyzer/casts-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/casts-1.c"
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
# 2 "./analyzer/casts-1.c" 2

struct s1
{
  char a;
  char b;
  char c;
  char d;
};

struct s2
{
  char arr[4];
};

struct s3
{
  struct inner {
    char a;
    char b;
  } arr[2];
};

void test_1 ()
{
  struct s1 x = {'A', 'B', 'C', 'D'};
  __analyzer_eval (x.a == 'A');
  __analyzer_eval (x.b == 'B');
  __analyzer_eval (x.c == 'C');
  __analyzer_eval (x.d == 'D');
  __analyzer_eval (((struct s2 *)&x)->arr[0] == 'A');
  __analyzer_eval (((struct s2 *)&x)->arr[1] == 'B');
  __analyzer_eval (((struct s2 *)&x)->arr[2] == 'C');
  __analyzer_eval (((struct s2 *)&x)->arr[3] == 'D');
  struct s3 *p3 = (struct s3 *)&x;
  __analyzer_eval (p3->arr[0].a == 'A');
  __analyzer_eval (p3->arr[0].b == 'B');
  __analyzer_eval (p3->arr[1].a == 'C');
  __analyzer_eval (p3->arr[1].b == 'D');

  ((struct s2 *)&x)->arr[1] = '#';
  __analyzer_eval (((struct s2 *)&x)->arr[1] == '#');
  __analyzer_eval (x.b == '#');
  __analyzer_eval (p3->arr[0].b == '#');
}

void test_2 ()
{
  struct s2 x = {{'A', 'B', 'C', 'D'}};
  __analyzer_eval (x.arr[0] == 'A');
  __analyzer_eval (x.arr[1] == 'B');
  __analyzer_eval (x.arr[2] == 'C');
  __analyzer_eval (x.arr[3] == 'D');
  struct s1 *p = (struct s1 *)&x;
  __analyzer_eval (p->a == 'A');
  __analyzer_eval (p->b == 'B');
  __analyzer_eval (p->c == 'C');
  __analyzer_eval (p->d == 'D');
}

void test_3 ()
{
  struct s3 x = {'A', 'B', 'C', 'D'};
  __analyzer_eval (x.arr[0].a == 'A');
  __analyzer_eval (x.arr[0].b == 'B');
  __analyzer_eval (x.arr[1].a == 'C');
  __analyzer_eval (x.arr[1].b == 'D');
  struct s1 *p1 = (struct s1 *)&x;
  __analyzer_eval (p1->a == 'A');
  __analyzer_eval (p1->b == 'B');
  __analyzer_eval (p1->c == 'C');
  __analyzer_eval (p1->d == 'D');
  struct s2 *p2 = (struct s2 *)&x;
  __analyzer_eval (p2->arr[0] == 'A');
  __analyzer_eval (p2->arr[1] == 'B');
  __analyzer_eval (p2->arr[2] == 'C');
  __analyzer_eval (p2->arr[3] == 'D');
}

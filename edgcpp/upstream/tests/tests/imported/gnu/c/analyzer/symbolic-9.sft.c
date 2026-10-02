//type: fp
//options: 
# 0 "./analyzer/symbolic-9.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/symbolic-9.c"
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
# 2 "./analyzer/symbolic-9.c" 2

struct st
{
  void *ptr[10];
  int arr[10];
};





struct st g;



struct st
test_conc_conc_ptr_conc_conc_arr (void)
{
  struct st s;
  s.ptr[1] = __builtin_malloc (1024);
  __analyzer_describe (0, s.ptr[1]);
  s.arr[5] = 42;
  __analyzer_describe (0, s.ptr[1]);
  __analyzer_describe (0, s.arr[5]);
  return s;
}

struct st
test_conc_conc_ptr_conc_sym_arr (int j)
{
  struct st s;
  s.ptr[1] = __builtin_malloc (1024);
  __analyzer_describe (0, s.ptr[1]);
  s.arr[j] = 42;
  __analyzer_describe (0, s.ptr[1]);
  __analyzer_describe (0, s.arr[j]);
  return s;
}

struct st
test_conc_conc_ptr_sym_conc_arr (struct st *p)
{
  struct st s;
  s.ptr[1] = __builtin_malloc (1024);
  __analyzer_describe (0, s.ptr[1]);
  p->arr[5] = 42;
  __analyzer_describe (0, s.ptr[1]);
  __analyzer_describe (0, p->arr[5]);
  return s;
}

struct st
test_conc_conc_ptr_sym_sym_arr (struct st *p, int j)
{
  struct st s;
  s.ptr[1] = __builtin_malloc (1024);
  __analyzer_describe (0, s.ptr[1]);
  p->arr[j] = 42;
  __analyzer_describe (0, s.ptr[1]);
  __analyzer_describe (0, p->arr[j]);
  return s;
}



void
test_sym_conc_ptr_conc_conc_arr (struct st *p)
{
  p->ptr[1] = __builtin_malloc (1024);
  __analyzer_describe (0, p->ptr[1]);
  g.arr[5] = 42;
  __analyzer_describe (0, p->ptr[1]);
  __analyzer_describe (0, g.arr[5]);
}

void
test_sym_conc_ptr_conc_sym_arr (struct st *p, int j)
{
  p->ptr[1] = __builtin_malloc (1024);
  __analyzer_describe (0, p->ptr[1]);
  g.arr[j] = 42;
  __analyzer_describe (0, p->ptr[1]);
  __analyzer_describe (0, g.arr[j]);
}

void
test_sym_conc_ptr_sym_conc_arr (struct st *p, struct st *q)
{
  p->ptr[1] = __builtin_malloc (1024);
  __analyzer_describe (0, p->ptr[1]);
  q->arr[5] = 42;
  __analyzer_describe (0, p->ptr[1]);
  __analyzer_describe (0, q->arr[5]);
}

void
test_sym_conc_ptr_sym_sym_arr (struct st *p, struct st *q, int j)
{
  p->ptr[1] = __builtin_malloc (1024);
  __analyzer_describe (0, p->ptr[1]);
  q->arr[j] = 42;
  __analyzer_describe (0, p->ptr[1]);
  __analyzer_describe (0, q->arr[j]);
}



struct st
test_conc_sym_ptr_conc_conc_arr (int i)
{
  struct st s;
  s.ptr[i] = __builtin_malloc (1024);
  __analyzer_describe (0, s.ptr[i]);
  s.arr[5] = 42;
  __analyzer_describe (0, s.ptr[i]);
  __analyzer_describe (0, s.arr[5]);
  return s;
}

struct st
test_conc_sym_ptr_conc_sym_arr (int i, int j)
{
  struct st s;
  s.ptr[i] = __builtin_malloc (1024);
  __analyzer_describe (0, s.ptr[i]);
  s.arr[j] = 42;
  __analyzer_describe (0, s.ptr[i]);
  __analyzer_describe (0, s.arr[j]);
  return s;
}

struct st
test_conc_sym_ptr_sym_conc_arr (int i, struct st *p)
{
  struct st s;
  s.ptr[i] = __builtin_malloc (1024);
  __analyzer_describe (0, s.ptr[i]);
  p->arr[5] = 42;
  __analyzer_describe (0, s.ptr[i]);
  __analyzer_describe (0, p->arr[5]);
  return s;
}

struct st
test_conc_sym_ptr_sym_sym_arr (int i, struct st *p, int j)
{
  struct st s;
  s.ptr[i] = __builtin_malloc (1024);
  __analyzer_describe (0, s.ptr[i]);
  p->arr[j] = 42;
  __analyzer_describe (0, s.ptr[i]);
  __analyzer_describe (0, p->arr[j]);
  return s;
}



void
test_sym_sym_ptr_conc_conc_arr (struct st *p, int i)
{
  p->ptr[i] = __builtin_malloc (1024);
  __analyzer_describe (0, p->ptr[i]);
  g.arr[5] = 42;
  __analyzer_describe (0, p->ptr[i]);
  __analyzer_describe (0, g.arr[5]);
}

void
test_sym_sym_ptr_conc_sym_arr (struct st *p, int i, int j)
{
  p->ptr[i] = __builtin_malloc (1024);
  __analyzer_describe (0, p->ptr[i]);
  g.arr[j] = 42;
  __analyzer_describe (0, p->ptr[i]);
  __analyzer_describe (0, g.arr[j]);
}

void
test_sym_sym_ptr_sym_conc_arr (struct st *p, int i, struct st *q)
{
  p->ptr[i] = __builtin_malloc (1024);
  __analyzer_describe (0, p->ptr[i]);
  q->arr[5] = 42;
  __analyzer_describe (0, p->ptr[i]);
  __analyzer_describe (0, q->arr[5]);
}

void
test_sym_sym_ptr_sym_sym_arr (struct st *p, int i, struct st *q, int j)
{
  p->ptr[i] = __builtin_malloc (1024);
  __analyzer_describe (0, p->ptr[i]);
  q->arr[j] = 42;
  __analyzer_describe (0, p->ptr[i]);
  __analyzer_describe (0, q->arr[j]);
}

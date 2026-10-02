//type: fp
//options: 
# 0 "./analyzer/symbolic-12.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/symbolic-12.c"
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
# 2 "./analyzer/symbolic-12.c" 2

void external_fn(void);

struct st_1
{
  char *name;
  unsigned size;
};

void test_1a (void *p, unsigned next_off)
{
  struct st_1 *r = p;

  external_fn();

  if (next_off >= r->size)
    return;

  if (next_off >= r->size)

    __analyzer_dump_path ();
}

void test_1b (void *p, unsigned next_off)
{
  struct st_1 *r = p;

  if (next_off >= r->size)
    return;

  if (next_off >= r->size)

    __analyzer_dump_path ();
}

void test_1c (struct st_1 *r, unsigned next_off)
{
  if (next_off >= r->size)
    return;

  if (next_off >= r->size)

    __analyzer_dump_path ();
}

void test_1d (struct st_1 *r, unsigned next_off)
{
  external_fn();

  if (next_off >= r->size)
    return;

  if (next_off >= r->size)

    __analyzer_dump_path ();
}

void test_1e (void *p, unsigned next_off)
{
  struct st_1 *r = p;

  while (1)
    {
      external_fn();

      if (next_off >= r->size)
 return;

      __analyzer_dump_path ();
    }
}

struct st_2
{
  char *name;
  unsigned arr[10];
};

void test_2a (void *p, unsigned next_off)
{
  struct st_2 *r = p;

  external_fn();

  if (next_off >= r->arr[5])
    return;

  if (next_off >= r->arr[5])

    __analyzer_dump_path ();
}

void test_2b (void *p, unsigned next_off, int idx)
{
  struct st_2 *r = p;

  external_fn();

  if (next_off >= r->arr[idx])
    return;

  if (next_off >= r->arr[idx])

    __analyzer_dump_path ();
}

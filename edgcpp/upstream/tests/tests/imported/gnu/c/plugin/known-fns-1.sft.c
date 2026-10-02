//type: fp
//options: 
# 0 "./plugin/known-fns-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./plugin/known-fns-1.c"




# 1 "./plugin/../analyzer/analyzer-decls.h" 1
# 20 "./plugin/../analyzer/analyzer-decls.h"
extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 44 "./plugin/../analyzer/analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);




extern long unsigned int __analyzer_get_strlen (const char *ptr);
# 6 "./plugin/known-fns-1.c" 2



extern int returns_42 (void);

void test_1 (void)
{
  int val = returns_42 ();
  __analyzer_eval (val == 42);
}



extern int
attempt_to_copy (void *to, const void *from, int sz);

void test_copy_success (void *to, const void *from, int sz)
{
  if (!attempt_to_copy (to, from, sz))
    {

    }
}

void test_copy_failure (void *to, const void *from, int sz)
{
  if (attempt_to_copy (to, from, sz))
    __analyzer_dump_path ();
}

struct coord
{
  int x;
  int y;
  int z;
};

void test_copy_2 (void)
{
  struct coord to = {1, 2, 3};
  struct coord from = {4, 5, 6};
  if (attempt_to_copy (&to, &from, sizeof (struct coord)))
    {

      __analyzer_eval (to.x == 1);
      __analyzer_eval (to.y == 2);
      __analyzer_eval (to.z == 3);
    }
  else
    {

      __analyzer_eval (to.x == 4);
      __analyzer_eval (to.y == 5);
      __analyzer_eval (to.z == 6);
    }
}

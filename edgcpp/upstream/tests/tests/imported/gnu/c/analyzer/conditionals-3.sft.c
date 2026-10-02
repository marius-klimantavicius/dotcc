//type: fp
//options: 
# 0 "./analyzer/conditionals-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/conditionals-3.c"


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
# 4 "./analyzer/conditionals-3.c" 2

static void __analyzer_only_called_when_flag_a_true (int i)
{
  __analyzer_eval (i == 42);
}

static void __analyzer_only_called_when_flag_b_true (int i)
{
  __analyzer_eval (i == 17);
}

int test_1 (int flag_a, int flag_b)
{
  int i = 17;

  __analyzer_eval (flag_a);
  __analyzer_eval (flag_b);

  if (flag_a)
    {
      __analyzer_eval (flag_a);
      __analyzer_eval (flag_b);
      i = 42;
    }

  __analyzer_eval (flag_b);

  if (flag_a)
    {
      __analyzer_eval (flag_a);
      __analyzer_eval (flag_b);
      __analyzer_eval (i == 42);
      __analyzer_eval (i == 17);
      __analyzer_only_called_when_flag_a_true (i);
    }
  else
    {
      __analyzer_eval (flag_a);
      __analyzer_eval (flag_b);
      __analyzer_eval (i == 42);
      __analyzer_eval (i == 17);
      __analyzer_only_called_when_flag_b_true (i);
    }
}

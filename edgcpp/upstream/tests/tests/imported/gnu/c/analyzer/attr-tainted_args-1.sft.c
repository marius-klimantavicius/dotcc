//type: fp
//options: 
# 0 "./analyzer/attr-tainted_args-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/attr-tainted_args-1.c"



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
# 5 "./analyzer/attr-tainted_args-1.c" 2

struct arg_buf
{
  int i;
  int j;
};



void __attribute__((tainted_args))
test_1 (int i, void *p, char *q)
{


  __analyzer_dump_exploded_nodes (0);

  __analyzer_dump_state ("taint", i);
  __analyzer_dump_state ("taint", p);
  __analyzer_dump_state ("taint", q);
  __analyzer_dump_state ("taint", *q);

  struct arg_buf *args = p;
  __analyzer_dump_state ("taint", args->i);
  __analyzer_dump_state ("taint", args->j);
}



struct s2
{
  void (*cb) (int, void *, char *)
    __attribute__((tainted_args));
};



void
test_2a (int i, void *p, char *q)
{


  __analyzer_dump_exploded_nodes (0);

  __analyzer_dump_state ("taint", i);
  __analyzer_dump_state ("taint", p);
  __analyzer_dump_state ("taint", q);

  struct arg_buf *args = p;
  __analyzer_dump_state ("taint", args->i);
  __analyzer_dump_state ("taint", args->j);
}



void
test_2b (int i, void *p, char *q)
{


  __analyzer_dump_exploded_nodes (0);
}


void
__analyzer_test_2c (int i, void *p, char *q)
{


  __analyzer_dump_exploded_nodes (0);

  __analyzer_dump_state ("taint", i);
  __analyzer_dump_state ("taint", p);
  __analyzer_dump_state ("taint", q);
}

struct s2 t2b =
{
  .cb = test_2b
};

struct s2 t2c =
{
  .cb = __analyzer_test_2c
};

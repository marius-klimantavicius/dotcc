//type: fp
//options: 
# 0 "./analyzer/paths-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/paths-4.c"
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
# 2 "./analyzer/paths-4.c" 2

struct state
{
  int mode;
  int data;
};

extern void do_stuff (struct state *, int);

int test_1 (struct state *s)
{
  __analyzer_dump_exploded_nodes (0);
  while (1)
    {
      __analyzer_dump_exploded_nodes (0);
      __analyzer_dump_exploded_nodes (0);

      do_stuff (s, s->mode);
    }
}

int test_2 (struct state *s)
{
  __analyzer_dump_exploded_nodes (0);
  while (1)
    {
      __analyzer_dump_exploded_nodes (0);
      __analyzer_dump_exploded_nodes (0);

      switch (s->mode)
 {
 case 0:
   __analyzer_dump_exploded_nodes (0);
   do_stuff (s, 0);
   break;
 case 1:
   __analyzer_dump_exploded_nodes (0);
   do_stuff (s, 17);
   break;
 case 2:
   __analyzer_dump_exploded_nodes (0);
   do_stuff (s, 5);
   break;
 case 3:
   __analyzer_dump_exploded_nodes (0);
   return 42;
 case 4:
   __analyzer_dump_exploded_nodes (0);
   return -3;
 }
    }
}

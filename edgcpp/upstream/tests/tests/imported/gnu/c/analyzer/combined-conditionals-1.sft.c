//type: fp
//options: 
# 0 "./analyzer/combined-conditionals-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/combined-conditionals-1.c"


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
# 4 "./analyzer/combined-conditionals-1.c" 2

extern int foo ();
extern int bar ();
extern int baz ();

void test_1 (int a, int b, int c)
{
  if (a && b && c)
    __analyzer_dump_path ();
}

void test_2 (int a, int b, int c)
{
  if (a && b)
    if (c)
      __analyzer_dump_path ();
}

void test_3 (int a, int b, int c)
{
  if (a)
    if (b && c)
      __analyzer_dump_path ();
}

void test_4 (void)
{
  while (foo () && bar ())
    __analyzer_dump_path ();
}

void test_5 (int a, int b, int c)
{
  if (a || b || c)
    {
    }
  else
    __analyzer_dump_path ();
}

void test_6 (void)
{
  int i;
  for (i = 0; i < 10 && foo (); i++)
    __analyzer_dump_path ();
}

int test_7 (void)
{
  if (foo () ? bar () ? baz () : 0 : 0)
    __analyzer_dump_path ();
}

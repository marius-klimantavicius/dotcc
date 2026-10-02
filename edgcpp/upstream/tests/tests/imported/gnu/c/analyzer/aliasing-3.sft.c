//type: fp
//options: 
# 0 "./analyzer/aliasing-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/aliasing-3.c"
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
# 2 "./analyzer/aliasing-3.c" 2



struct s1
{
  int f1;
};

static struct s1 *p1_glob = ((void *)0);

void test_1 (struct s1 **pp1, struct s1 *p1_parm)
{
  struct s1 *init_p1_glob = p1_glob;

  __analyzer_eval (p1_glob == init_p1_glob);

  if (!p1_glob)
    return;

  __analyzer_eval (p1_glob == init_p1_glob);
  __analyzer_eval (p1_glob != ((void *)0));

  *pp1 = p1_parm;



  __analyzer_eval (p1_glob == init_p1_glob);
  __analyzer_eval (p1_glob != ((void *)0));
}

struct s2
{
  int f1;
};

static struct s2 *p2_glob = ((void *)0);

void test_2 (struct s2 **pp2, struct s2 *p2_parm)
{

  p2_glob = __builtin_malloc (sizeof (struct s2));
  if (!p2_glob)
    return;

  __analyzer_eval (p2_glob != ((void *)0));

  *pp2 = p2_parm;



  __analyzer_eval (p2_glob != ((void *)0));
}

struct s3
{
  int f1;
};

struct s3 *p3_glob = ((void *)0);

void test_3 (struct s3 **pp3, struct s3 *p3_parm)
{
  p3_glob = __builtin_malloc (sizeof (struct s3));
  if (!p3_glob)
    return;

  __analyzer_eval (p3_glob != ((void *)0));

  *pp3 = p3_parm;



  __analyzer_eval (p3_glob != ((void *)0));
}

//type: fp
//options: 
# 0 "./analyzer/unknown-fns-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/unknown-fns-3.c"


# 1 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef int wchar_t;
# 425 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
} max_align_t;
# 4 "./analyzer/unknown-fns-3.c" 2
# 1 "./analyzer/analyzer-decls.h" 1








# 8 "./analyzer/analyzer-decls.h"
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
# 5 "./analyzer/unknown-fns-3.c" 2

void unknown_fn (void *);

void test_1 (void)
{
  int i;
  unknown_fn (&i);
  if (i)
    __analyzer_eval (i);
  else
    __analyzer_eval (i);
  __analyzer_dump_exploded_nodes (0);
}

struct foo
{
  int i;
  int j;
};

void test_2 (void)
{
  struct foo f;
  unknown_fn (&f);
  if (f.j)
    __analyzer_eval (f.j);
  else
    __analyzer_eval (f.j);
  __analyzer_dump_exploded_nodes (0);
}

void test_3 (int flag)
{
  int i;
  unknown_fn (&i);
  if (i)
    {
      __analyzer_eval (i);
      if (flag)
 __analyzer_eval (flag);
      else
 __analyzer_eval (flag);
    }
  else
    __analyzer_eval (i);
  if (flag)
    __analyzer_eval (flag);
  else
    __analyzer_eval (flag);
  __analyzer_dump_exploded_nodes (0);
}

void test_4 (int y)
{
  int x;
  unknown_fn (&x);
  if (x)
    {
      __analyzer_eval (x);
      x = 0;
    }
  __analyzer_dump_exploded_nodes (0);
}

//type: fp
//options: 
# 0 "./analyzer/asm-x86-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/asm-x86-1.c"


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
# 4 "./analyzer/asm-x86-1.c" 2

int test_out (void)
{
  int dst_a, dst_b;
  asm ("mov 42, %0"
       : "=r" (dst_a));
  asm ("mov 42, %0"
       : "=r" (dst_b));
  __analyzer_eval (dst_a == dst_b);
  return dst_a;
}

int test_out_in (int src_a)
{
  int dst_a, dst_b;
  asm ("mov %1, %0"
       : "=r" (dst_a)
       : "r" (src_a));
  asm ("mov %1, %0"
       : "=r" (dst_b)
       : "r" (src_a));
  __analyzer_eval (dst_a == dst_b);
  return dst_a;
}

int test_out_in_in (int src_a, int src_b)
{
  int dst_a, dst_b;
  asm ("mov %1, %0;\n"
       "add %2, %0"
       : "=r" (dst_a)
       : "r" (src_a),
  "r" (src_b));
  asm ("mov %1, %0;\n"
       "add %2, %0"
       : "=r" (dst_b)
       : "r" (src_a),
  "r" (src_b));
  __analyzer_eval (dst_a == dst_b);
  return dst_a;
}

void test_inout_1 (int v)
{
  int saved = v;
  int result_a, result_b;
  asm ("dec %0"
       : "+r" (v));
  result_a = v;

  asm ("dec %0"
       : "+r" (v));
  result_b = v;

  __analyzer_eval (v == saved);
  __analyzer_eval (v == result_a);
  __analyzer_eval (v == result_b);
}

void test_inout_2 (void)
{
  int v;
  int result_a, result_b;
  asm ("dec %0"
       : "+r" (v));
}

//type: fp
//options: 
# 0 "./analyzer/torture/fold-ptr-arith-pr105784.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/torture/fold-ptr-arith-pr105784.c"


# 1 "./analyzer/torture/../analyzer-decls.h" 1
# 20 "./analyzer/torture/../analyzer-decls.h"
extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 44 "./analyzer/torture/../analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);




extern long unsigned int __analyzer_get_strlen (const char *ptr);
# 4 "./analyzer/torture/fold-ptr-arith-pr105784.c" 2

extern _Bool quit_flag;
extern void char_charset (int);

static void
__analyzer_ccl_driver (int *source, int src_size)
{
  int *src = source, *src_end = src + src_size;
  int i = 0;

  while (!quit_flag)
    {
      if (src < src_end)
 {
   __analyzer_dump_path ();
   i = *src++;
 }
      char_charset (i);
    }
}

void
Fccl_execute_on_string (char *str, long str_bytes)
{
  while (1)
    {
      char *p = str;
      char *endp = str + str_bytes;
      int source[1024];
      int src_size = 0;

      while (src_size < 1024 && p < endp)
 {
   __analyzer_dump_path ();
   source[src_size++] = *p++;
 }

      __analyzer_ccl_driver (source, src_size);
    }
}
